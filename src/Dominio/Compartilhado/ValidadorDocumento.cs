namespace DeliveryApp.Dominio.Compartilhado;

public static class ValidadorDocumento
{
    public static bool CpfValido(string cpf)
    {
        string documento = Normalizar(cpf);

        if (documento.Length != 11 || PossuiDigitosIguais(documento))
            return false;

        return CalcularDigito(documento[..9], 10) == documento[9] - '0' &&
               CalcularDigito(documento[..10], 11) == documento[10] - '0';
    }

    public static bool CnpjValido(string cnpj)
    {
        string documento = Normalizar(cnpj);

        if (documento.Length != 14 || PossuiDigitosIguais(documento))
            return false;

        int primeiroDigito = CalcularDigitoCnpj(documento[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        int segundoDigito = CalcularDigitoCnpj(documento[..13], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        return primeiroDigito == documento[12] - '0' && segundoDigito == documento[13] - '0';
    }

    public static bool CpfOuCnpjValido(string documento)
    {
        string valorNormalizado = Normalizar(documento);

        return valorNormalizado.Length == 11
            ? CpfValido(valorNormalizado)
            : CnpjValido(valorNormalizado);
    }

    public static string Normalizar(string documento)
    {
        return new string(documento.Where(char.IsDigit).ToArray());
    }

    private static bool PossuiDigitosIguais(string documento)
    {
        return documento.All(digito => digito == documento[0]);
    }

    private static int CalcularDigito(string parte, int pesoInicial)
    {
        int soma = parte.Select((digito, indice) => (digito - '0') * (pesoInicial - indice)).Sum();
        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }

    private static int CalcularDigitoCnpj(string parte, IReadOnlyList<int> pesos)
    {
        int soma = parte.Select((digito, indice) => (digito - '0') * pesos[indice]).Sum();
        int resto = soma % 11;

        return resto < 2 ? 0 : 11 - resto;
    }
}
