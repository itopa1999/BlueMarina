
namespace BlueMarina.Domain.Entities;
public enum Gender { 
    Male,
    Female, 
    Other 
}

public enum MaritalStatus { 
    Single, 
    Married, 
    Divorced, 
    Widowed 

}
public enum RiskAppetite { 
    Conservative, 
    Moderate, 
    Aggressive, 
    Income 
}


public enum KycLevel { 
    Basic = 1, 
    Intermediate = 2, 
    Advanced = 3 
}

public enum AddressType { 
    Home, 
    Business, 
    Mailing 
}

public enum Currency { 
    NGN, 
    USD, 
    GBP, 
    EUR 
}

public enum DocumentType { 
    Passport, 
    DriversLicense, 
    VoterCard, 
    NationalId, 
    UtilityBill, 
    BankStatement 
}


public enum DocumentStatus { 
    Pending, 
    Verified, 
    Rejected, 
    Expired 
}

public enum VerificationType { 
    Bvn, 
    Nin, 
    Pep, 
    Sanctions, 
    Address, 
    Email, 
    Phone, 
    Liveness 
}

public enum VerificationResult { 
    Pass, 
    Fail, 
    Pending, 
    Error 
}