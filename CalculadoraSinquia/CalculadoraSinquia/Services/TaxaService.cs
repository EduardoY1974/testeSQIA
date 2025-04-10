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
    }
}
