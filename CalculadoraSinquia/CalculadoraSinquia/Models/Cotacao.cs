namespace CalculadoraSinqia.Models
{
    public class CotacaoItem
    {
        public int id { get; set; }
        public DateTime data { get; set; }
        public string? indexador { get; set; }
        public decimal valor { get; set; }
    }
}
