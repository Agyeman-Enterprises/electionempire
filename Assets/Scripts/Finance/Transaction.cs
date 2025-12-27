// File: Assets/Scripts/Finance/Transaction.cs
// Add this file to your project

using System;
using UnityEngine;

namespace ElectionEmpire.Finance
{
    /// <summary>
    /// Represents a financial transaction in the campaign finance system
    /// </summary>
    [Serializable]
    public class Transaction
    {
        public string TransactionId { get; set; }
        public TransactionType Type { get; set; }
        public float Amount { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public TransactionStatus Status { get; set; }
        public bool IsReported { get; set; }
        public bool IsDarkMoney { get; set; }
        public string DonorId { get; set; }
        public string Category { get; set; }

        // Additional properties for compliance checking
        public string SourceAccount { get; set; }
        public string DestinationAccount { get; set; }
        public string RelatedEntityId { get; set; }
        public string RelatedEntityName { get; set; }
        public bool IsSuspicious { get; set; }
        
        public Transaction()
        {
            TransactionId = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Status = TransactionStatus.Pending;
        }
        
        public Transaction(TransactionType type, float amount, string description) : this()
        {
            Type = type;
            Amount = amount;
            Description = description;
        }
        
        public Transaction(TransactionType type, float amount, string source, string destination, string description) : this()
        {
            Type = type;
            Amount = amount;
            Source = source;
            Destination = destination;
            Description = description;
        }
    }
    
    /// <summary>
    /// Result of a transaction attempt
    /// </summary>
    [Serializable]
    public class TransactionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Transaction Transaction { get; set; }
        public float NewBalance { get; set; }
        public TransactionErrorCode ErrorCode { get; set; }
        public string Violation { get; set; }
        
        public static TransactionResult Successful(Transaction transaction, float newBalance, string message = "Transaction completed successfully")
        {
            return new TransactionResult
            {
                Success = true,
                Message = message,
                Transaction = transaction,
                NewBalance = newBalance,
                ErrorCode = TransactionErrorCode.None
            };
        }
        
        public static TransactionResult Failed(string message, TransactionErrorCode errorCode = TransactionErrorCode.Unknown)
        {
            return new TransactionResult
            {
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
        
        public static TransactionResult InsufficientFunds(float available, float required)
        {
            return new TransactionResult
            {
                Success = false,
                Message = $"Insufficient funds. Available: ${available:N2}, Required: ${required:N2}",
                ErrorCode = TransactionErrorCode.InsufficientFunds
            };
        }
    }
    
    /// <summary>
    /// Types of financial transactions
    /// </summary>
    public enum TransactionType
    {
        None = 0,
        
        // Income
        Donation,
        PACContribution,
        Fundraiser,
        SelfFunding,
        PartySupport,
        DarkMoney,
        MatchingFunds,
        
        // Expenses
        Advertising,
        StaffSalary,
        EventCost,
        TravelExpense,
        OfficeExpense,
        ConsultantFee,
        PollExpense,
        DirtyTrickCost,
        Expense,  // Generic expense type
        
        // Transfers
        Transfer,
        Refund,
        
        // Special
        BribePayment,
        HushMoney,
        LegalFee,
        FinePayment
    }
    
    /// <summary>
    /// Status of a transaction
    /// </summary>
    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Refunded,
        UnderInvestigation
    }
    
    /// <summary>
    /// Error codes for failed transactions
    /// </summary>
    public enum TransactionErrorCode
    {
        None = 0,
        InsufficientFunds,
        InvalidAmount,
        InvalidSource,
        InvalidDestination,
        DuplicateTransaction,
        RateLimitExceeded,
        ComplianceViolation,
        FrozenAccount,
        Unknown
    }
}
