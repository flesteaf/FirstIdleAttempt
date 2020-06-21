using Moq;
using TMPro;

namespace Assets.Scripts.UnitTests.Commands
{
    public static class SceneMocking
    {
        public static void SetupMoneyText(this MoneyText moneyText)
        {
            moneyText.Money = new Mock<TextMeshProUGUI>().Object;
        }

        public static void SetupConsoleText(this ConsoleText consoleText)
        {
            consoleText.Console = new Mock<TextMeshProUGUI>().Object;
        }
    }
}
