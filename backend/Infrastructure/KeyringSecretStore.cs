using System.Diagnostics;

namespace CorsoApi.Infrastructure
{
    public class KeyringSecretStore : ISecretStore
    {
        public async Task<string> GetRequiredSecretAsync(string keyName, CancellationToken cancellationToken = default)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "secret-tool",
                Arguments = $"lookup corso {keyName}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi) ??
                throw new InvalidOperationException("Failed to start secret-tool process.");

            string? output = await process.StandardOutput.ReadToEndAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Secret not found for key: {keyName}");
            await process.WaitForExitAsync(cancellationToken);

            return output.Trim();
        }
    }
}