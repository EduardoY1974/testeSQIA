using CalculadoraSinqia.Models;
using CalculadoraSinqia.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class CalculadoraSQIAController : ControllerBase
{
    private readonly TaxaService _taxaService;
    private readonly ILogger<CalculadoraSQIAController> _logger;

    public CalculadoraSQIAController(TaxaService taxaService, ILogger<CalculadoraSQIAController> logger)
    {
        _taxaService = taxaService;
        _logger = logger;
    }

    [HttpGet("calcularDebug")]
    public IActionResult CalcularInvestimentoDebug(
        [FromQuery] decimal valorInvestido,
        [FromQuery] DateTime dataAplicacao,
        [FromQuery] DateTime dataFinal)
    {
        _logger.LogInformation("Requisição para calcular investimento recebida.");

        if (valorInvestido <= 0)
        {
            _logger.LogWarning("Valor investido inválido: {ValorInvestido}", valorInvestido);
            return BadRequest("O valor investido deve ser maior que zero.");
        }

        if (dataAplicacao >= dataFinal)
        {
            _logger.LogWarning("Valor data de aplicação inválida.");
            return BadRequest("A data de aplicação deve ser anterior à data final.");
        }

        var taxasDiariasComAnual = _taxaService.ObterTaxasDiariasComAnual(dataAplicacao, dataFinal);

        if (taxasDiariasComAnual == null || !taxasDiariasComAnual.Any())
        {
            _logger.LogWarning("Taxas nao encontradas para o periodo.");
            return NotFound("Não foram encontradas taxas para o período especificado.");
        }

        decimal fatorAcumulado = 1.0m;
        DateTime dataCorrente = dataAplicacao;
        List<ResultadoCalculo> resultados = new List<ResultadoCalculo>();

        while (dataCorrente <= dataFinal)
        {
            var taxaDiaInfo = taxasDiariasComAnual.FirstOrDefault(t => t.Data.Date == dataCorrente.Date);

            if (taxaDiaInfo != null)
            {
                decimal taxaDiariaParaExibir = 1;
                if (dataCorrente > dataAplicacao)
                {
                    fatorAcumulado *= taxaDiaInfo.FatorDiario;
                    taxaDiariaParaExibir = taxaDiaInfo.FatorDiario;
                }

                resultados.Add(new ResultadoCalculo
                {
                    DataReferencia = dataCorrente,
                    TaxaAnual = taxaDiaInfo.TaxaAnual,
                    TaxaDiaria = taxaDiariaParaExibir,
                    TaxaAcumulada = fatorAcumulado,
                    ValorAtualizado = Math.Round(valorInvestido * fatorAcumulado, 2)
                });
            }
            else
            {
                // Lógica para lidar com dias sem taxa, se necessário
                resultados.Add(new ResultadoCalculo
                {
                    DataReferencia = dataCorrente,
                    TaxaAnual = 0, // Ou algum valor padrão
                    TaxaDiaria = 1,
                    TaxaAcumulada = fatorAcumulado,
                    ValorAtualizado = Math.Round(valorInvestido * fatorAcumulado, 2)
                });
            }

            dataCorrente = dataCorrente.AddDays(1);
        }
        _logger.LogInformation("Cálculo concluído com sucesso.");
        return Ok(resultados);
    }
    [HttpGet("calcular")]
    public IActionResult CalcularInvestimento(
    [FromQuery] decimal valorInvestido,
    [FromQuery] DateTime dataAplicacao,
    [FromQuery] DateTime dataFinal)
    {
        _logger.LogInformation("Requisição para calcular investimento recebida.");

        if (valorInvestido <= 0)
        {
            _logger.LogWarning("Valor investido inválido: {ValorInvestido}", valorInvestido);
            return BadRequest("O valor investido deve ser maior que zero.");
        }

        if (dataAplicacao >= dataFinal)
        {
            _logger.LogWarning("Valor data de aplicação inválida.");
            return BadRequest("A data de aplicação deve ser anterior à data final.");
        }

        var taxasDiarias = _taxaService.ObterTaxasDiariasComAnual(dataAplicacao, dataFinal);

        if (taxasDiarias == null || !taxasDiarias.Any())
        {
            _logger.LogWarning("Taxas nao encontradas para o periodo.");
            return NotFound("Não foram encontradas taxas diárias para o período especificado.");
        }

        decimal fatorAcumulado = 1.0m;
        DateTime dataCalculo = dataAplicacao;
        List<ResultadoCalculo> resultados = new List<ResultadoCalculo>();

        while (dataCalculo <= dataFinal)
        {
            var taxaDiaInfo = taxasDiarias.FirstOrDefault(t => t.Data.Date == dataCalculo.Date);

            if (taxaDiaInfo != null)
            {
                if (dataCalculo > dataAplicacao) // Rentabilidade a partir do segundo dia
                {
                    fatorAcumulado *= taxaDiaInfo.FatorDiario;
                }

                resultados.Add(new ResultadoCalculo
                {
                    DataReferencia = dataCalculo,
                    TaxaAnual = taxaDiaInfo.TaxaAnual,
                    TaxaDiaria = (dataCalculo == dataAplicacao) ? 1 : taxaDiaInfo.FatorDiario,
                    TaxaAcumulada = (dataCalculo == dataAplicacao) ? 1 : fatorAcumulado,
                    ValorAtualizado = (dataCalculo == dataAplicacao) ? valorInvestido : Math.Round(valorInvestido * fatorAcumulado, 2)
                });
            }

            dataCalculo = dataCalculo.AddDays(1);
        }

        var resultado = new
        {
            FatorAcumulado = resultados.Last().TaxaAcumulada,
            ValorAtualizado = resultados.Last().ValorAtualizado
        };

        _logger.LogInformation("Cálculo concluído com sucesso.");
        return Ok(resultado);
    }
}