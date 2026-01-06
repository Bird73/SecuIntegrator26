declare module 'lunar-javascript' {
    export class Lunar {
        static fromDate(date: Date): Lunar;
        getDayInChinese(): string;
        getMonthInChinese(): string;
    }
}
