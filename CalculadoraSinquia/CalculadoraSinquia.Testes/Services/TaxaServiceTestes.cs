using CalculadoraSinqia.Data;
using CalculadoraSinqia.Models;
using CalculadoraSinqia.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CalculadoraSinquia.Testes.Services
{
    public class TaxaServiceTestes
    {
        [Fact]
        public void ObterTaxasDiariasComAnual_PeriodoValido_RetornaListaDeTaxasComAnual()
        {
            // Arrange
            var mockContext = new Mock<CotacaoContext>();
            var dataInicio = new DateTime(2025, 3, 13);
            var dataFim = new DateTime(2025, 3, 15);

            mockContext.Setup(c => c.Cotacao)
                .Returns((Microsoft.EntityFrameworkCore.DbSet<CotacaoItem>)MockHelpers.GetQueryableMockDbSet(new List<CotacaoItem>
                {
                new CotacaoItem { id = 1, data = dataInicio, indexador = "SQI", valor = 12 },
                new CotacaoItem { id = 2, data = dataInicio.AddDays(1), indexador = "SQI", valor = 12.5m },
                new CotacaoItem { id = 3, data = dataFim, indexador = "SQI", valor = 11 }
                }));

            var taxaService = new TaxaService(mockContext.Object, null); // Pass null para o logger para este teste

            // Act
            var taxas = taxaService.ObterTaxasDiariasComAnual(dataInicio, dataFim);

            // Assert
            Assert.IsNotNull(taxas);
            Assert.AreEqual(3, taxas.Count);
            Assert.AreEqual(dataInicio.Date, taxas[0].Data.Date);
            Assert.AreEqual(12, taxas[0].TaxaAnual);
            Assert.AreEqual(Math.Round((decimal)Math.Pow((double)(1 + 12 / 100.0), 1.0 / 252.0), 8), taxas[0].FatorDiario);
            Assert.AreEqual(dataInicio.AddDays(1).Date, taxas[1].Data.Date);
            Assert.AreEqual(12.5m, taxas[1].TaxaAnual);
            Assert.AreEqual(Math.Round((decimal)Math.Pow((double)(1 + 12.5 / 100.0), 1.0 / 252.0), 8), taxas[1].FatorDiario);
            Assert.AreEqual(dataFim.Date, taxas[2].Data.Date);
            Assert.AreEqual(11, taxas[2].TaxaAnual);
            Assert.AreEqual(Math.Round((decimal)Math.Pow((double)(1 + 11 / 100.0), 1.0 / 252.0), 8), taxas[2].FatorDiario);
        }

        [Fact]
        public void ObterTaxasDiariasComAnual_PeriodoSemCotacoesSQI_RetornaListaVazia()
        {
            // Arrange
            var mockContext = new Mock<CotacaoContext>();
            var dataInicio = new DateTime(2025, 3, 13);
            var dataFim = new DateTime(2025, 3, 15);

            mockContext.Setup(c => c.Cotacao)
                .Returns((Microsoft.EntityFrameworkCore.DbSet<CotacaoItem>)MockHelpers.GetQueryableMockDbSet(new List<CotacaoItem>
                {
                new CotacaoItem { id = 1, data = dataInicio, indexador = "OUTRO", valor = 12 },
                new CotacaoItem { id = 2, data = dataInicio.AddDays(1), indexador = "OUTRO", valor = 12.5m }
                }));

            var taxaService = new TaxaService(mockContext.Object, null);

            // Act
            var taxas = taxaService.ObterTaxasDiariasComAnual(dataInicio, dataFim);

            // Assert
            Assert.IsNotNull(taxas);
            Assert.AreEqual(taxas, null);
        }

        [Fact]
        public void ObterTaxasDiariasComAnual_PeriodoForaDasDatasDasCotacoes_RetornaListaVazia()
        {
            // Arrange
            var mockContext = new Mock<CotacaoContext>();
            var dataInicio = new DateTime(2025, 3, 16);
            var dataFim = new DateTime(2025, 3, 18);

            mockContext.Setup(c => c.Cotacao)
                .Returns((Microsoft.EntityFrameworkCore.DbSet<CotacaoItem>)MockHelpers.GetQueryableMockDbSet(new List<CotacaoItem>
                {
                new CotacaoItem { id = 1, data = new DateTime(2025, 3, 13), indexador = "SQI", valor = 12 }
                }));

            var taxaService = new TaxaService(mockContext.Object, null);

            // Act
            var taxas = taxaService.ObterTaxasDiariasComAnual(dataInicio, dataFim);

            // Assert
            Assert.IsNotNull(taxas);
            Assert.AreEqual(taxas, null);
        }

        [Fact]
        public void ObterTaxasDiariasComAnual_ConsultaOrdenadaPorData()
        {
            // Arrange
            var mockContext = new Mock<CotacaoContext>();
            var data1 = new DateTime(2025, 3, 15);
            var data2 = new DateTime(2025, 3, 13);
            var data3 = new DateTime(2025, 3, 14);

            mockContext.Setup(c => c.Cotacao)
                .Returns((Microsoft.EntityFrameworkCore.DbSet<CotacaoItem>)MockHelpers.GetQueryableMockDbSet(new List<CotacaoItem>
                {
                new CotacaoItem { id = 1, data = data1, indexador = "SQI", valor = 10 },
                new CotacaoItem { id = 2, data = data2, indexador = "SQI", valor = 11 },
                new CotacaoItem { id = 3, data = data3, indexador = "SQI", valor = 12 }
                }));

            var taxaService = new TaxaService(mockContext.Object, null);

            // Act
            var taxas = taxaService.ObterTaxasDiariasComAnual(data2, data1); // Intencionalmente fora de ordem

            // Assert   
            Assert.IsNotNull(taxas);
            Assert.AreEqual(3, taxas.Count);
            Assert.AreEqual(data2.Date, taxas[0].Data.Date);
            Assert.AreEqual(data3.Date, taxas[1].Data.Date);
            Assert.AreEqual(data1.Date, taxas[2].Data.Date);
        }
    }

    // Helper para criar um Mock DbSet (útil para testes com Entity Framework Core)
    public static class MockHelpers
    {
        public static IQueryable<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<T>>();
            mockSet.As<System.Linq.IQueryable>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<System.Linq.IQueryable>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<System.Linq.IQueryable>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<System.Linq.IQueryable>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            return mockSet.Object;
        }
    }
}