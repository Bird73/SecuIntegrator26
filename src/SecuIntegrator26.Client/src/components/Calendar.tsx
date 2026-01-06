import React, { useMemo } from 'react';
import { Lunar } from 'lunar-javascript';
import './Calendar.css';

interface CalendarProps {
    year: number;
    month: number; // 1-12
    holidays: Set<string>; // Set of 'YYYY-MM-DD'
    onDateClick: (date: string) => void;
}

const Calendar: React.FC<CalendarProps> = ({ year, month, holidays, onDateClick }) => {
    const days = useMemo(() => {
        const result = [];
        const firstDay = new Date(year, month - 1, 1);
        const lastDay = new Date(year, month, 0);
        const daysInMonth = lastDay.getDate();
        const startDayOfWeek = firstDay.getDay(); // 0 (Sun) - 6 (Sat)

        // Empty slots for previous month
        for (let i = 0; i < startDayOfWeek; i++) {
            result.push(null);
        }

        // Days
        for (let d = 1; d <= daysInMonth; d++) {
            result.push(d);
        }

        return result;
    }, [year, month]);

    const getLunarDate = (day: number) => {
        try {
            const date = new Date(year, month - 1, day);
            const lunar = Lunar.fromDate(date);
            const dayText = lunar.getDayInChinese();
            // If it's the first day (初一), show the month instead (e.g. 正月, 二月)
            if (dayText === '初一') {
                return `${lunar.getMonthInChinese()}月`;
            }
            return dayText;
        } catch (e) {
            return '';
        }
    };

    const formatDate = (day: number) => {
        return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
    };

    return (
        <div className="calendar-month">
            <h3>{year}年 {month}月</h3>
            <div className="calendar-grid">
                <div className="weekday">日</div>
                <div className="weekday">一</div>
                <div className="weekday">二</div>
                <div className="weekday">三</div>
                <div className="weekday">四</div>
                <div className="weekday">五</div>
                <div className="weekday">六</div>
                {days.map((day, idx) => {
                    if (day === null) return <div key={`empty-${idx}`} className="day empty"></div>;

                    const dateStr = formatDate(day);
                    const isHoliday = holidays.has(dateStr);
                    const lunarText = getLunarDate(day);

                    return (
                        <div
                            key={day}
                            className={`day ${isHoliday ? 'holiday' : ''}`}
                            onClick={() => onDateClick(dateStr)}
                        >
                            <div className="solar">{day}</div>
                            <div className="lunar">{lunarText}</div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
};

export default Calendar;
