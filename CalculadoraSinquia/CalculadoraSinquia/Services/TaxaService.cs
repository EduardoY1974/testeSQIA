using CalculadoraSinqia.Data;
using CalculadoraSinqia.Models;
using System;
using System.Collections.Generic;
using System.Linq;


namespace CalculadoraSinqia.Services
{
    public class TaxaService
    {
        private readonly CotacaoContext _context;

        public TaxaService(CotacaoContext context)
        {
            _context = context;
        }

        public List<TaxaComAnual> ObterTaxasDiariasComAnual(DateTime inicio, DateTime fim)
        {
            return _context.Cotacao
                .Where(c => c.data >= inicio && c.data <= fim && c.indexador == "SQI")
                .OrderBy(c => c.data)
                .Select(c => new TaxaComAnual
                {
                    Data = c.data,
                    TaxaAnual = c.valor,
                    FatorDiario = Math.Round((decimal)Math.Pow((double)(1 + c.valor / 100), 1.0 / 252.0), 8)
                })
                .ToList();
        }

        public DateTime ObterDiaUtilAnterior(DateTime data)
        {
            DateTime diaAnterior = data.AddDays(-1);
            while (diaAnterior.DayOfWeek == DayOfWeek.Saturday || diaAnterior.DayOfWeek == DayOfWeek.Sunday)
            {
                diaAnterior = diaAnterior.AddDays(-1);
            }
            return diaAnterior;
        }

        public decimal TruncateDecimal(decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            decimal temp = Math.Truncate(value * step);
            return temp / step;
        }
    }
}
