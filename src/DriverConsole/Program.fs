open System
open Loans
open System.Diagnostics.Metrics

let loanTransactions =
    [|
        {
            TransactionDate = DateOnly(2021, 03, 31)
            Amount = 2_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 04, 23)
            Amount = 3_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 04, 12)
            Amount = 10_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 12, 02)
            Amount = 2_000_000M
            TransactionType = MoneyLent
        }

    |]

let rateChanges =
    [|
        {
            RateStartDate = DateOnly(2022,01,28)
            RateInterval = Annually
            Rate = 0.1250M
        }
        {
            RateStartDate = DateOnly(2021,03,31)
            RateInterval = Annually
            Rate = 0.1200M
        }
        {
            RateStartDate = DateOnly(2022,03,25)
            RateInterval = Annually
            Rate = 0.1275M
        }
        {
            RateStartDate = DateOnly(2022,05,20)
            RateInterval = Annually
            Rate = 0.1325M
        }
        {
            RateStartDate = DateOnly(2022,07,21)
            RateInterval = Annually
            Rate = 0.1400M
        }

    |]

let loan =
    { RateChanges =  rateChanges; LoanTransactions = loanTransactions }

let annualEquivalentRate =
    convertRateDefinitionInterval( rateChanges[0], Annually)


type loanCalculationErrors =
    | RateStartDateTooLate

let calculateInterestChangeLoanAtoms(loan:Loan,calculationEndDate:DateOnly) =
    let sortedRateChanges =  loan.RateChanges |> Array.sortBy(fun x -> x.RateStartDate)
    let sortedTransactions = loan.LoanTransactions |> Array.sortBy(fun x -> x.TransactionDate)
    let firstTransactionDate = sortedTransactions.[0].TransactionDate
    let firstKnownRateDate = sortedRateChanges.[0].RateStartDate
    let result =
        if (firstKnownRateDate > firstTransactionDate) then
            Error RateStartDateTooLate
        else
            // Create a transaction atom for each rate change
            let atomsCount = sortedRateChanges.Length
            let createAtomForIndexedItem(sortedRateChanges:InterestRateDefinition[],index,calculationEndDate) =
                let item = sortedRateChanges[index]
                let dailyRate = convertRateDefinitionInterval(item,Daily)
                let startDate = item.RateStartDate
                let endDate =
                    if (index = sortedRateChanges.Length-1) then
                        calculationEndDate
                    else
                        sortedRateChanges[index+1].RateStartDate.AddDays(-1)
                let result =
                    {
                        StartDate = startDate
                        EndDate = endDate
                        OpeningBalance = 0.0M
                        EffectiveDailyRate = dailyRate.Rate
                    }
                result

            let atoms = Array.init atomsCount (fun index -> createAtomForIndexedItem(sortedRateChanges,index,calculationEndDate))
            Ok atoms
    result


let calculateTransactionLoanAtoms(loan:Loan,calculationEndDate:DateOnly) =
    let sortedTransactions = loan.LoanTransactions |> Array.sortBy(fun x -> x.TransactionDate)
    let atomsCount = sortedTransactions.Length
    let createAtomForIndexedTransaction(sortedTransactions:LoanTransaction[],index:int,calculationEndDate:DateOnly)=
        let item = sortedTransactions[index]
        //Todo: Start Here
        >>
        let dailyRate = -1.0M //Todo: figure out how to get active daily rate at this point.
        let startDate = item.TransactionDate
        let endDate =
            if (index = sortedTransactions.Length-1) then
                  calculationEndDate
              else
                  sortedTransactions[index+1].TransactionDate.AddDays(-1)
        let openingBalance = item.Amount
        let result =
            {
                StartDate = startDate
                EndDate = endDate
                OpeningBalance = openingBalance
                EffectiveDailyRate = dailyRate
            }
        result
    let atoms = Array.init atomsCount (fun index -> createAtomForIndexedTransaction(sortedTransactions,index,calculationEndDate))
    atoms

let loanCalculationEndDate = DateOnly(2022,07,26)

let interestChangeAtoms = calculateInterestChangeLoanAtoms(loan, loanCalculationEndDate)
let transactionAtoms = calculateTransactionLoanAtoms(loan, loanCalculationEndDate)


let mutable interest = 0.0M
for transaction in loan.LoanTransactions do

    interest <-  interest + dailyCompoundedInterestForInterval(transaction.Amount ,annualEquivalentRate.Rate,DateOnly(2021, 10, 21),DateOnly(2021, 10, 26))


for transaction in loanTransactions do
    printfn "%s" (transaction.TransactionDate.ToShortDateString())
