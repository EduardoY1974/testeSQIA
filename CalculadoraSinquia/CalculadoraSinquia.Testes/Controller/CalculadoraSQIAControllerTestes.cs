// CalculadoraSinquia.Testes/Controllers/CalculadoraSQIAControllerTestes.cs
using CalculadoraSinqia.Models;
using CalculadoraSinqia.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace CalculadoraSinquia.Testes.Controller
{
    public class CalculadoraSQIAControllerTestes
    {
        [Fact]
        public void CalcularInvestimento_ValorInvestidoZero_RetornaBadRequest()
        {
            // Arrange
            var mockTaxaService = new Mock<TaxaService>();
            var controller = new CalculadoraSQIAController(mockTaxaService.Object, null); // Pass null para o logger para este teste

            // Act
            var result = controller.CalcularInvestimento(0, DateTime.Now.AddDays(-10), DateTime.Now);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CalcularInvestimento_DataAplicacaoIgualDataFinal_RetornaBadRequest()
        {
            // Arrange
            var mockTaxaService = new Mock<TaxaService>();
            var controller = new CalculadoraSQIAController(mockTaxaService.Object, null);
            var dataHoje = DateTime.Now;

            // Act
            var result = controller.CalcularInvestimento(1000, dataHoje, dataHoje);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CalcularInvestimento_DataAplicacaoPosteriorDataFinal_RetornaBadRequest()
        {
            // Arrange
            var mockTaxaService = new Mock<TaxaService>();
            var controller = new CalculadoraSQIAController(mockTaxaService.Object, null);
            var dataAplicacao = DateTime.Now.AddDays(5);
            var dataFinal = DateTime.Now;

            // Act
            var result = controller.CalcularInvestimento(1000, dataAplicacao, dataFinal);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void CalcularInvestimento_TaxasNaoEncontradas_RetornaNotFound()
        {
            // Arrange
            var mockTaxaService = new Mock<TaxaService>();
            mockTaxaService.Setup(service => service.ObterTaxasDiariasComAnual(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(new List<TaxaComAnual>()); // Simula não encontrando taxas

            var controller = new CalculadoraSQIAController(mockTaxaService.Object, null);

            // Act
            var result = controller.CalcularInvestimento(1000, DateTime.Now.AddDays(-2), DateTime.Now);

            // Assert
            Assert.IsInstanceOfType<NotFoundObjectResult>(result);
        }
    }
}