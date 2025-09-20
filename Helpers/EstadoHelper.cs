using System.Text;
using System.Globalization;
namespace backend.Helpers;
public static class EstadoHelper
{
    public static string NormalizarEstado(string estado)
    {
        if (string.IsNullOrWhiteSpace(estado))
            return string.Empty;

        // Remove acentos e coloca em minúsculas
        string normalized = RemoveAcentos(estado.ToLower().Trim());

        // Mapeamento dos estados brasileiros
        var estados = new Dictionary<string, string>
        {
            {"acre", "AC"},
            {"alagoas", "AL"},
            {"amapa", "AP"},
            {"amazonas", "AM"},
            {"bahia", "BA"},
            {"ceara", "CE"},
            {"distrito federal", "DF"},
            {"espirito santo", "ES"},
            {"goias", "GO"},
            {"maranhao", "MA"},
            {"mato grosso", "MT"},
            {"mato grosso do sul", "MS"},
            {"minas gerais", "MG"},
            {"para", "PA"},
            {"paraiba", "PB"},
            {"parana", "PR"},
            {"pernambuco", "PE"},
            {"piaui", "PI"},
            {"rio de janeiro", "RJ"},
            {"rio grande do norte", "RN"},
            {"rio grande do sul", "RS"},
            {"rondonia", "RO"},
            {"roraima", "RR"},
            {"santa catarina", "SC"},
            {"sao paulo", "SP"},
            {"sergipe", "SE"},
            {"tocantins", "TO"}
        };

        // Verifica se já é uma sigla válida
        if (normalized.Length == 2 && estados.ContainsValue(normalized.ToUpper()))
            return normalized.ToUpper();

        // Tenta encontrar o estado no dicionário
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        if (estados.TryGetValue(normalized, out string sigla))
            return sigla;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        // Se não encontrou, retorna a string normalizada (sem acentos, em maiúsculas)
        return normalized.Length > 2 ? normalized.ToUpper() : normalized.ToUpper();
    }

    private static string RemoveAcentos(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}