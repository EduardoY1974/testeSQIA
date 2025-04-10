namespace CalculadoraSinqia.Models
{
    public class ResultadoCalculo
    {
        public DateTime DataReferencia { get; set; }
        public decimal TaxaAnual { get; set; }
        public decimal TaxaDiaria { get; set; }
        public decimal TaxaAcumulada { get; set; }
        public decimal ValorAtualizado { get; set; }
    }
}
