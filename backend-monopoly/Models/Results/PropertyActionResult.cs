namespace MonopolyBackend.Models.Results
{
    /// <summary>
    /// Domain result for property-related actions (buy, build, sell, mortgage)
    /// </summary>
    public class PropertyActionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
