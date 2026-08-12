namespace LightInvestTest;

public record OrderRequest(
    string Symbol,
    decimal Price,
    int Volume,
    string UserId
);
