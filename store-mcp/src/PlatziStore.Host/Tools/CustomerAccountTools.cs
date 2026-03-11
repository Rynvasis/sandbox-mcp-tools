using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CustomerAccountTools
{
    [McpServerTool(Name = "list_store_customers")]
    [Description("Lists all customer profiles registered in the store.")]
    public static async Task<string> ListStoreCustomers(
        ICustomerAccountService service)
    {
        var outcome = await service.ListCustomersAsync();
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCustomerList);
    }

    [McpServerTool(Name = "get_customer_by_id")]
    [Description("Gets detailed profile information for a specific customer by their integer ID.")]
    public static async Task<string> GetCustomerById(
        ICustomerAccountService service,
        [Description("The integer ID of the customer.")] int customerId)
    {
        var outcome = await service.GetCustomerByIdAsync(customerId);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCustomerProfile);
    }

    [McpServerTool(Name = "register_customer")]
    [Description("Registers a new customer account in the store.")]
    public static async Task<string> RegisterCustomer(
        ICustomerAccountService service,
        [Description("The full name of the customer.")] string name,
        [Description("The email address of the customer.")] string email,
        [Description("The plaintext password for the customer account.")] string password,
        [Description("A valid avatar image URL for the customer.")] string avatar)
    {
        var payload = new CustomerRegistration
        {
            Name = name,
            Email = email,
            Password = password,
            Avatar = avatar
        };
        var outcome = await service.RegisterCustomerAsync(payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCustomerProfile);
    }

    [McpServerTool(Name = "update_customer_profile")]
    [Description("Updates an existing customer profile. Provide the customer ID and any fields you want to change.")]
    public static async Task<string> UpdateCustomerProfile(
        ICustomerAccountService service,
        [Description("The integer ID of the customer to update.")] int customerId,
        [Description("New full name for the customer.")] string? name = null,
        [Description("New email address. Note: If changed, it must be available.")] string? email = null,
        [Description("New password for the customer.")] string? password = null,
        [Description("New avatar image URL.")] string? avatar = null)
    {
        var payload = new CustomerRegistration
        {
            Name = name ?? string.Empty,
            Email = email ?? string.Empty,
            Password = password ?? string.Empty,
            Avatar = avatar ?? string.Empty
        };
        var outcome = await service.UpdateCustomerProfileAsync(customerId, payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCustomerProfile);
    }

    [McpServerTool(Name = "check_email_availability")]
    [Description("Checks if an email address is available for registration (i.e. not already in use).")]
    public static async Task<string> CheckEmailAvailability(
        ICustomerAccountService service,
        [Description("The email address to check.")] string email)
    {
        var outcome = await service.CheckEmailAvailabilityAsync(email);
        return outcome.IsSuccess 
            ? (outcome.Data ? $"Email '{email}' is available." : $"Email '{email}' is NOT available (already registered).")
            : $"Error checking email availability: {outcome.ErrorMessage}";
    }
}
