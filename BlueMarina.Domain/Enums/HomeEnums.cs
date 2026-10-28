namespace BlueMarina.Domain.Enums;

public enum NotificationCategory
{
    Trade,
    CorporateAction,
    Security,
    Wallet,
    Savings,
    Approval,
    Loan,
    Learn,
    Support,
    General
}

public enum NotificationActionType
{
    None,
    Review,
    ViewOrder,
    ViewTransaction,
    ApprovePayment,
    ViewLoan,
    OpenLesson,
    Custom
}

public enum AccountType
{
    Wallet,
    Savings,
    Investment
}

public enum TransactionType
{
    Funding,
    Withdrawal,
    Buy,
    Sell,
    Interest,
    Dividend,
    Transfer,
    Conversion,
    LoanDisbursement,
    LoanRepayment,
    Fee,
    Reversal,
    SavingsDeposit,
    SavingsWithdrawal
}

public enum TransactionStatus
{
    Processing,
    Completed,
    Failed,
    Reversed
}

public enum TransactionChannel
{
    BankTransferNip,
    InternalTransfer,
    Card,
    DirectDebit,
    System
}

public enum RiskProfileType
{
    Conservative,
    Balanced,
    Growth
}

public enum ContactChangeField
{
    Phone,
    Email
}

public enum ContactChangeStatus
{
    Pending,
    Confirmed,
    Expired,
    Cancelled
}

public enum StatementKind
{
    Account,
    Portfolio
}

public enum StatementAccount
{
    Wallet,
    Savings,
    Investments
}

public enum StatementPaymentType
{
    All,
    Deposits,
    Withdrawals,
    BuysAndSells,
    DividendsAndInterest
}

public enum StatementFormat
{
    Pdf,
    Csv
}

public enum StatementStatus
{
    Preparing,
    Ready,
    Empty,
    Failed
}

public enum ConsentType
{
    MarketingEmails,
    CrsSelfCertification,
    TermsOfUse,
    PrivacyPolicy
}
