module Loans

open System

type LoanTransactionType =
    | MoneyLent
    | MoneyRepaid

type LoanTransaction =
    { TransactionDate: DateTime
      Amount: Decimal
      TransactionType: LoanTransactionType }

type RateInterval =
    | Daily
    | Weekly
    | Monthly
    | Annually

type InterestRateDefinition =
    { RateInterval: RateInterval
      Rate: Decimal }

type RateChange =
    { Date: Date
      RateDefinition: InterestRateDefinition }



let loanTransactions =
    [ { TransactionDate = DateTime(2021, 1, 1)
        Amount = 200M
        TransactionType = MoneyLent }
      { TransactionDate = DateTime(2022, 1, 1)
        Amount = 100M
        TransactionType = MoneyRepaid } ]
