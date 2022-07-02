open System
open Loans

let loanTransactions =
    [
        {
            TransactionDate = DateOnly(2021, 10, 21)
            Amount = 2_000_000M
            TransactionType = MoneyLent
        }
        {
            TransactionDate = DateOnly(2021, 10, 26)
            Amount = 10_000M
            TransactionType = MoneyRepaid
        }
        {
            TransactionDate = DateOnly(2021, 11, 30)
            Amount = 30_000M
            TransactionType = MoneyRepaid
        }
        {
            TransactionDate = DateOnly(2021, 12, 22)
            Amount = 30_000M
            TransactionType = MoneyRepaid
        }

    ]

for transaction in loanTransactions do
    printfn "%s" (transaction.TransactionDate.ToShortDateString())
