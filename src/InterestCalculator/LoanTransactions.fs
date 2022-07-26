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

type InterestCalculationAtom =
    { StartDate: DateOnly
      EndDate: DateOnly
      OpeningBalance: Decimal
      EffectiveDailyRate: Decimal
    }

type RateIntervalType =
    | Daily
    | Monthly
    | Annually


type InterestRateDefinition =
    {
      RateStartDate: DateOnly
      RateInterval: RateIntervalType
      Rate: Decimal }


type Loan =
    {
        RateChanges: List<InterestRateDefinition>
        LoanTransactions: List<LoanTransaction>
    }




/// Convert the InterestRateDefinition to use an equivalent rate with a different interval
let convertRateDefinitionInterval(interestRateDefinition:InterestRateDefinition,targetInterval:RateIntervalType) =
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
                {
                    RateStartDate = interestRateDefinition.RateStartDate
                    RateInterval = Monthly;
                    Rate = monthlyRate
                }
            | Annually ->
                let annualRate = interestRateDefinition.Rate * 365M
                {
                    RateStartDate = interestRateDefinition.RateStartDate
                    RateInterval = Annually;
                    Rate = annualRate
                }
        | Monthly ->
            match targetInterval with
            | Daily ->
                let annualRate = interestRateDefinition.Rate * 12M
                let dailyRate = annualRate / 365M
                {
                    RateStartDate = interestRateDefinition.RateStartDate
                    RateInterval = targetInterval;
                    Rate = dailyRate}
            | Monthly ->
                interestRateDefinition
            | Annually ->
                let annualRate = interestRateDefinition.Rate * 12M
                {RateStartDate = interestRateDefinition.RateStartDate;RateInterval = targetInterval; Rate = annualRate}
        | Annually ->
            match targetInterval with
            | Daily ->
                let dailyRate = interestRateDefinition.Rate/365M
                {RateStartDate = interestRateDefinition.RateStartDate;RateInterval = targetInterval; Rate = dailyRate}
            | Monthly ->
                let monthlyRate = interestRateDefinition.Rate/12M
                {RateStartDate = interestRateDefinition.RateStartDate;RateInterval = targetInterval; Rate = monthlyRate}
            | Annually ->
                interestRateDefinition
    result

let dailyCompoundedInterestForInterval(openingBalance:decimal,dailyRate:Decimal,startDate:DateOnly,endDate:DateOnly):Decimal =
    // See this stackoverflow answer.
    //https://stackoverflow.com/questions/6425501/is-there-a-math-api-for-powdecimal-decimal
    // .net decided to not implement pow for decimal, leading to these ridiculous hacks
  let precisionMagnifier = (double)1_000_000.0
  let numberOfDays = (double)(endDate.DayNumber  - startDate.DayNumber)
  let exponent = numberOfDays/365.*1.0  //Assumes incoming rate = annual
  let doubleResult = (double)openingBalance * (1.0 + (double(dailyRate)))**(exponent)-(double)openingBalance
  let increasedPrecisionResult = (doubleResult * precisionMagnifier)
  let intResult = (decimal)increasedPrecisionResult
  let result = (decimal)intResult / (decimal)precisionMagnifier
  result

