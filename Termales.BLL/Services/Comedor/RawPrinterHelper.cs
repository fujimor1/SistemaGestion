using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Termales.BLL.Services.Comedor;

/// <summary>
/// Envía bytes crudos (ESC/POS) a una impresora instalada en Windows (USB/local),
/// sin pasar por el renderizado GDI. Patrón estándar de Microsoft vía winspool.drv.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class RawPrinterHelper
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
        [MarshalAs(UnmanagedType.LPStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
    }

    [DllImport("winspool.drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA di);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.drv", SetLastError = true)]
    private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

    public static void SendBytesToPrinter(string nombreImpresora, byte[] bytes)
    {
        if (string.IsNullOrWhiteSpace(nombreImpresora))
            throw new InvalidOperationException("No se configuró el nombre de la impresora USB (ImpresoraComanda:NombreImpresora)");

        if (!OpenPrinter(nombreImpresora, out var hPrinter, IntPtr.Zero))
            throw new InvalidOperationException($"No se pudo abrir la impresora '{nombreImpresora}' (¿el nombre coincide con el de Windows?)");

        try
        {
            var di = new DOCINFOA
            {
                pDocName = "Comanda",
                pOutputFile = null,
                pDataType = "RAW",
            };

            if (!StartDocPrinter(hPrinter, 1, ref di))
                throw new InvalidOperationException("No se pudo iniciar el documento de impresión (StartDocPrinter)");

            try
            {
                if (!StartPagePrinter(hPrinter))
                    throw new InvalidOperationException("No se pudo iniciar la página de impresión (StartPagePrinter)");

                var pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                try
                {
                    Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                    if (!WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out _))
                        throw new InvalidOperationException("No se pudo escribir en la impresora (WritePrinter)");
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pUnmanagedBytes);
                }

                EndPagePrinter(hPrinter);
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            ClosePrinter(hPrinter);
        }
    }
}
