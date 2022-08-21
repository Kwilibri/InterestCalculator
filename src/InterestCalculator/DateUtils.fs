module DateUtils

    open System


    let calculateMonthEndsInInterval(startDate: DateOnly, endDate:DateOnly) =
        let mutable currentDate= startDate
        let mutable dateList:DateOnly list = []
        while currentDate <= endDate do
            let lastDayOfMonth = DateTime.DaysInMonth(currentDate.Year,currentDate.Month)
            currentDate <- DateOnly(currentDate.Year,currentDate.Month,lastDayOfMonth)
            dateList <-
                if currentDate <= endDate then
                    dateList @ [currentDate]
                else
                    dateList
            currentDate <- currentDate.AddDays(1)
        dateList


