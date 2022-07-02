module Loans

open System

(* Terms and Definitions
 Nominal interest rates does not take into account the compounding period.
 Effective interest rate takes compounding period into account
 i.e. if compounding period is same as period for stated interest rate then
 effective and nominal interest rates are the same, else not.

 *)



type LoanTransactionType =
    | MoneyLent
    | MoneyRepaid

type LoanTransaction =
    { TransactionDate: DateOnly
      Amount: Decimal
      TransactionType: LoanTransactionType }

type RateInterval =
    | Daily
    | Monthly
    | Annually


type InterestRateDefinition =
    { RateInterval: RateInterval
      Rate: Decimal }


type Loan =
    {
        RateDefinition: InterestRateDefinition
        LoanTransactions: List<LoanTransaction>
    }




/// Convert the InterestRateDefinition to use an equivalent rate with a different interval
let convertRateDefinitionInterval(interestRateDefinition:InterestRateDefinition,targetInterval:RateInterval) =
    let originalInterval = interestRateDefinition.RateInterval
    let result =
        match originalInterval with
        | Daily ->
            match targetInterval with
            | Daily ->
                // No conversion required - return original
                interestRateDefinition
            | Monthly ->
                // First convert to annual
                let annualRate = interestRateDefinition.Rate * 365M
                let monthlyRate = annualRate / 12M
                { RateInterval = Monthly; Rate = monthlyRate }
            | Annually ->
                let annualRate = interestRateDefinition.Rate * 365M
                { RateInterval = Annually; Rate = annualRate }
        | Monthly ->
            match targetInterval with
            | Daily ->
                let annualRate = interestRateDefinition.Rate * 12M
                let dailyRate = annualRate / 365M
                { RateInterval = targetInterval; Rate = dailyRate}
            | Monthly ->
                interestRateDefinition
            | Annually ->
                let annualRate = interestRateDefinition.Rate * 12M
                {RateInterval = targetInterval; Rate = annualRate}
        | Annually ->
            match targetInterval with
            | Daily ->
                let dailyRate = interestRateDefinition.Rate/365M
                {RateInterval = targetInterval; Rate = dailyRate}
            | Monthly ->
                let monthlyRate = interestRateDefinition.Rate/12M
                {RateInterval = targetInterval; Rate = monthlyRate}
            | Annually ->
                interestRateDefinition
    result

