using CorsoApi.Core;

namespace CorsoApiTests;

public class ChatPromptTests
{
    [Fact(DisplayName = "it should inject accounts into prompt")]
    public void ContextIntoPrompt()
    {
        var accounts = new List<Account>{ 
            new ()
            {
                Id = 123456,
                Name = "test-account1",
                Username = "username-1",
                Password = "any"
            },
            new ()
            {
                Id = 123456,
                Name = "test-account2",
                Username = "username-2",
                Password = "any"
            }
        };

        var actual = ChatPrompt.CreateInstructions(accounts);

        Assert.Contains("test-account1", actual);
        Assert.Contains("test-account2", actual);
        Assert.Contains("username-1", actual);
        Assert.Contains("username-2", actual);
    }
    
    [Fact(DisplayName = "it should hide secrets from context")]
    public void ContextWithoutSecrets()
    {
        const string password = "private-password-6a1e";
        
        var accounts = new List<Account>
        {
            new()
            {
                Id = 123456,
                Name = "Email",
                Username = "Any",
                Password = password
            }
        };

        var actual = ChatPrompt.CreateInstructions(accounts);
        
        Assert.DoesNotContain(password, actual);
        Assert.DoesNotContain("123456", actual);
    }
}
