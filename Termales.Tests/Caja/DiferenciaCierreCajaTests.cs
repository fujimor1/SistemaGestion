using Termales.BLL.Services;
using Termales.Entities.Models.Caja;

namespace Termales.Tests.Caja;

public class DiferenciaCierreCajaTests
{
    // Apertura 100, ventas 500 efectivo + 300 yape, egresos 50 → esperado: efectivo 550, yape 300.
    private static CierreCaja Cierre(decimal efectivoFisico, decimal yapeFisico, decimal transferencia = 0) => new()
    {
        MontoApertura = 100,
        TotalSistema = 800,
        EfectivoSistema = 500,
        YapeSistema = 300,
        TotalEgresos = 50,
        EfectivoFisico = efectivoFisico,
        YapeFisico = yapeFisico,
        TransferenciaFisico = transferencia,
    };

    [Fact]
    public void SinCierre_NoHayDiferencia()
    {
        Assert.Equal((null, null), DiferenciaCierreCaja.Calcular(null));
    }

    [Fact]
    public void CajaCuadrada_AmbasEnCero()
    {
        var (ef, ya) = DiferenciaCierreCaja.Calcular(Cierre(550, 300));
        Assert.Equal(0m, ef);
        Assert.Equal(0m, ya);
    }

    [Fact]
    public void FaltanteSoloEnEfectivo_NoTocaYape()
    {
        var (ef, ya) = DiferenciaCierreCaja.Calcular(Cierre(545, 300));
        Assert.Equal(-5m, ef);
        Assert.Equal(0m, ya);
    }

    [Fact]
    public void SobranteSoloEnYape_NoTocaEfectivo()
    {
        var (ef, ya) = DiferenciaCierreCaja.Calcular(Cierre(550, 302));
        Assert.Equal(0m, ef);
        Assert.Equal(2m, ya);
    }

    [Fact]
    public void FaltanteEnUnoYSobranteEnOtro_SeVenPorSeparado()
    {
        var (ef, ya) = DiferenciaCierreCaja.Calcular(Cierre(540, 305));
        Assert.Equal(-10m, ef);
        Assert.Equal(5m, ya);
    }

    [Fact]
    public void Transferencia_CuentaDelLadoYape()
    {
        var (ef, ya) = DiferenciaCierreCaja.Calcular(Cierre(550, 250, transferencia: 50));
        Assert.Equal(0m, ef);
        Assert.Equal(0m, ya);
    }

    [Fact]
    public void Egresos_BajanLoEsperadoEnEfectivo()
    {
        // Si el cajero contó 600 (sin descontar los 50 de egresos), sobran 50 en efectivo.
        var (ef, _) = DiferenciaCierreCaja.Calcular(Cierre(600, 300));
        Assert.Equal(50m, ef);
    }

    [Theory]
    [InlineData(550, 300, 0)]
    [InlineData(545, 300, 0)]
    [InlineData(540, 305, 0)]
    [InlineData(520, 290, 7)]
    public void LaSumaDeLasDosCoincideConLaDiferenciaTotal(decimal efectivoFisico, decimal yapeFisico, decimal transferencia)
    {
        var cierre = Cierre(efectivoFisico, yapeFisico, transferencia);
        // Misma fórmula que CajaService.CerrarCajaAsync: físico total − (apertura + sistema − egresos).
        var diferenciaTotal = (efectivoFisico + yapeFisico + transferencia)
            - (cierre.MontoApertura + cierre.TotalSistema - cierre.TotalEgresos);

        var (ef, ya) = DiferenciaCierreCaja.Calcular(cierre);

        Assert.Equal(diferenciaTotal, ef!.Value + ya!.Value);
    }

    [Fact]
    public void CierreViejoSinDesglose_NoSePuedeRepartir()
    {
        var viejo = new CierreCaja { TotalSistema = 800, EfectivoSistema = 0, YapeSistema = 0, EfectivoFisico = 800 };
        Assert.Equal((null, null), DiferenciaCierreCaja.Calcular(viejo));
    }

    [Fact]
    public void DiaSinVentas_SiSeReparte()
    {
        // Sin ventas el desglose en 0 es legítimo: solo se compara apertura − egresos.
        var sinVentas = new CierreCaja { MontoApertura = 100, TotalEgresos = 30, EfectivoFisico = 70 };
        var (ef, ya) = DiferenciaCierreCaja.Calcular(sinVentas);
        Assert.Equal(0m, ef);
        Assert.Equal(0m, ya);
    }
}
