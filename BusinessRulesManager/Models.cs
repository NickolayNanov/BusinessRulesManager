namespace BusinessRulesManager.Models
{
    public interface IRulesEngineModel
    {
    }

    public class Account : IRulesEngineModel
    {
        public decimal Balance { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public AccountType AccountType { get; set; }
        public int TransactionCount { get; set; }
        public int CreditScore { get; set; }
        public decimal LastTransactionAmount { get; set; }
        public int AccountAgeInYears { get; set; }
        public int NumberOfOverdrafts { get; set; }
        public decimal AverageMonthlyDeposit { get; set; }
    }

    public enum AccountType
    {
        Savings,
        Checking
    }

    public class Loan : IRulesEngineModel
    {
        public decimal LoanAmount { get; set; }
        public double InterestRate { get; set; }
        public int RepaymentTermInYears { get; set; }
        public LoanType LoanType { get; set; }
        public DateTime StartDate { get; set; }
        public bool IsSecured { get; set; }
        public int CreditScoreAtTimeOfApplication { get; set; }
        public decimal MonthlyRepaymentAmount { get; set; }
    }

    public enum LoanType
    {
        Personal,
        Mortgage,
        Auto
    }
    public class Customer : IRulesEngineModel
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public CustomerType Type { get; set; }
        public List<Account> Accounts { get; set; }
    }

    public enum CustomerType
    {
        Individual,
        Corporate
    }

    public class Transaction : IRulesEngineModel
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public TransactionType Type { get; set; }
        public string SourceAccountId { get; set; }
        public string DestinationAccountId { get; set; }
    }

    public enum TransactionType
    {
        Deposit,
        Withdrawal,
        Transfer
    }
    public class CreditCard : IRulesEngineModel
    {
        public string CardNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public double InterestRate { get; set; }
        public DateTime DueDate { get; set; }
        public int CreditScore { get; set; }
        public bool IsActive { get; set; }
    }
    public class InvestmentAccount : IRulesEngineModel
    {
        public decimal PortfolioValue { get; set; }
        public RiskProfile Profile { get; set; }
        public List<Investment> Investments { get; set; }
    }

    public enum RiskProfile
    {
        Low,
        Medium,
        High
    }

    public class Investment
    {
        public string InvestmentId { get; set; }
        public string Name { get; set; }
        public InvestmentType Type { get; set; }
        public decimal AmountInvested { get; set; }
    }

    public enum InvestmentType
    {
        Stocks,
        Bonds,
        MutualFunds
    }

}
