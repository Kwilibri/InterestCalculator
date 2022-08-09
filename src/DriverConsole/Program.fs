open System
open Loans
open System.Diagnostics.Metrics
open System.Diagnostics

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
            RateStartDate = DateOnly(2021,11,19)
            RateInterval = Annually
            Rate = 0.1225M
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
    | TargetDateTooEarly
    | TargetDateTooLate


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

let activeRateForGivenDate(interestCalculationAtoms:InterestCalculationAtom[],targetDate:DateOnly) =
    let count = interestCalculationAtoms.Length
    if (targetDate < interestCalculationAtoms[0].StartDate) then
        Error TargetDateTooEarly
    elif (targetDate > interestCalculationAtoms[count-1].EndDate) then
        Error TargetDateTooLate
    else
        let mutable candidate = 0.0M
        let mutable iterator = 0
        let mutable startDate = interestCalculationAtoms[iterator].StartDate
        let mutable endDate = interestCalculationAtoms[iterator].EndDate
        let maxDate = interestCalculationAtoms[count-1].EndDate

        while (endDate <= maxDate) do
            if (targetDate >= startDate && targetDate <= endDate) then
                candidate <- interestCalculationAtoms[iterator].EffectiveDailyRate
            iterator <- iterator + 1
            if (iterator <= count-1) then
                startDate <- interestCalculationAtoms[iterator].StartDate
                endDate <- interestCalculationAtoms[iterator].EndDate
            else
                endDate <- maxDate.AddDays(1)
        Ok candidate


let createAtomForIndexedTransaction(
        sortedTransactions:LoanTransaction[],
        interestChangeAtoms:InterestCalculationAtom[],
        index:int,
        calculationEndDate:DateOnly)=
    let item = sortedTransactions[index]
    let dailyRateResult = activeRateForGivenDate(interestChangeAtoms,item.TransactionDate)
    let dailyRate =
        match dailyRateResult with
        | Ok item -> item
        | Error err -> failwith "Couldn't calculate the applicable interest rate"
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




let calculateTransactionLoanAtoms(loan:Loan,calculationEndDate:DateOnly) =
    let sortedTransactions = loan.LoanTransactions |> Array.sortBy(fun x -> x.TransactionDate)
    let atomsCount = sortedTransactions.Length
    let interestChangeAtomsResult = calculateInterestChangeLoanAtoms(loan, calculationEndDate)
    let interestChangeAtoms =
        match interestChangeAtomsResult with
        | Ok item -> item
        | Error err -> failwith "incorrect Date"

    let atoms = Array.init atomsCount (fun index -> createAtomForIndexedTransaction(sortedTransactions,interestChangeAtoms,index,calculationEndDate))
    atoms


let loanCalculationEndDate = DateOnly(2022,07,26)
let interestChangeAtomsResult = calculateInterestChangeLoanAtoms(loan, loanCalculationEndDate)
let transactionAtoms = calculateTransactionLoanAtoms(loan, loanCalculationEndDate)

let interestChangeAtoms =
    match interestChangeAtomsResult with
    | Ok item -> item
    | Error _ -> failwith "There was an error calculation interest change transactions"
printfn "Printing interest rate transaction atoms"
for item in interestChangeAtoms do
    printfn "%A" item

printfn "--------------------------------------------------------------------------------"
printfn "Printing transaction atoms"
for transaction in transactionAtoms do
    printfn "%A" transaction


// Suggested starting point when next working on the system.
// Merge the interest change transactions atoms and the transaction atoms into one series of atoms.
// Also, we need to create atoms such as the month end balance transactions, that also needs to be merged into the series.
