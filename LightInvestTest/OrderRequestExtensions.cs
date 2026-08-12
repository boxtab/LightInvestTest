namespace LightInvestTest;

public static class OrderRequestExtensions
{
    // Метод возвращает список ошибок или пустой список, если всё ок
    public static List<string> Validate(this OrderRequest? request)
    {
        var errors = new List<string>();

        if (request == null)
        {
            errors.Add("Request cannot be null.");
            return errors; // Дальше проверять нет смысла
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            errors.Add("UserId is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            errors.Add("Symbol is required.");
        }

        if (request.Price <= 0)
        {
            errors.Add("Price must be greater than zero.");
        }

        if (request.Volume <= 0)
        {
            errors.Add("Volume must be greater than zero.");
        }

        return errors;
    }
}
