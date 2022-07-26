open System
open Loans

let loanTransactions =
    [
        {
            TransactionDate = DateOnly(2021, 03, 31)
            Amount = 2_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 04, 12)
            Amount = 10_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 04, 23)
            Amount = 3_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 12, 02)
            Amount = 2_000_000M
            TransactionType = MoneyLent
        }

    ]

let rateDefinition =
    [
        {
            RateStartDate = DateOnly(2021,03,31)
            RateInterval = Annually
            Rate = 0.1200M
        }
        {
            RateStartDate = DateOnly(2022,01,28)
            RateInterval = Annually
            Rate = 0.1250M
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




    ]

let loan =
    { RateChanges =  rateDefinition; LoanTransactions = loanTransactions }

let annualEquivalentRate =
    convertRateDefinitionInterval( rateDefinition.Head, Annually)


let loanScheduleTargetDate = DateOnly(2022,06,30)
let calculateLoanAtoms(loan:Loan) =
    /// Need to calculate loan atom for each transaction and each interest change.


let mutable interest = 0.0M
for transaction in loan.LoanTransactions do

    interest <-  interest + dailyCompoundedInterestForInterval(transaction.Amount ,annualEquivalentRate.Rate,DateOnly(2021, 10, 21),DateOnly(2021, 10, 26))


for transaction in loanTransactions do
    printfn "%s" (transaction.TransactionDate.ToShortDateString())
