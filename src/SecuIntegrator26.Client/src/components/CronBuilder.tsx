import React, { useEffect, useState } from 'react';

interface CronBuilderProps {
    value: string;
    onChange: (value: string) => void;
    onClose: () => void;
}

type Frequency = 'daily' | 'weekly' | 'monthly' | 'yearly' | 'custom';

// Helper to pad numbers
const pad = (n: number) => n.toString().padStart(2, '0');

const CronBuilder: React.FC<CronBuilderProps> = ({ value, onChange, onClose }) => {
    const [frequency, setFrequency] = useState<Frequency>('daily');

    // Daily State
    const [dailyType, setDailyType] = useState<'once' | 'interval'>('once');
    const [dailyTime, setDailyTime] = useState('00:00');
    const [dailyInterval, setDailyInterval] = useState(1);
    const [dailyIntervalUnit, setDailyIntervalUnit] = useState<'hour' | 'minute'>('hour');

    // Weekly State
    const [weeklyTime, setWeeklyTime] = useState('00:00');
    const [weeklyDays, setWeeklyDays] = useState<number[]>([]);

    // Monthly State
    const [monthlyType, setMonthlyType] = useState<'day' | 'range'>('day');
    const [monthlyTime, setMonthlyTime] = useState('00:00');
    const [monthlyDay, setMonthlyDay] = useState(1); // Start Day
    const [monthlyEndDay, setMonthlyEndDay] = useState(1); // End Day for range
    const [monthlyInterval, setMonthlyInterval] = useState(1);
    const [monthlyStartMonth, setMonthlyStartMonth] = useState(new Date().getMonth() + 1); // Default to current month

    // Yearly State
    const [yearlyTime, setYearlyTime] = useState('00:00');
    const [yearlyMonth, setYearlyMonth] = useState(1);
    const [yearlyDay, setYearlyDay] = useState(1);

    // Custom State
    const [customCron, setCustomCron] = useState(value);

    // Initial Parse
    useEffect(() => {
        tryParseCron(value);
    }, []); // Run once on mount

    const tryParseCron = (cron: string) => {
        const parts = cron.split(' ');
        if (parts.length < 6) {
            setFrequency('custom');
            setCustomCron(cron);
            return;
        }

        const [sec, min, hour, day, month, dayOfWeek] = parts;

        // Check Daily
        // Once: "0 M H * * ?"
        if (day === '*' && month === '*' && (dayOfWeek === '?' || dayOfWeek === '*')) {
            setFrequency('daily');
            setDailyType('once');
            setDailyTime(`${pad(parseInt(hour))}:${pad(parseInt(min))}`);
            return;
        }

        // Interval: "0 0/M H-H * * ?" or similar is complex to parse perfectly back to UI 
        // without knowing exact intention. For now, simple Daily Once detection.
        // If we can't detect simple patterns, default to custom or try others.

        // Check Weekly
        // "0 M H ? * MON,TUE"
        if (day === '?' && month === '*') {
            setFrequency('weekly');
            setWeeklyTime(`${pad(parseInt(hour))}:${pad(parseInt(min))}`);
            // Parse dayOfWeek
            // Simple support for 1,2,3 or MON,TUE
            // For this MVP, if it looks complex, go Custom
            if (dayOfWeek !== '*' && dayOfWeek !== '?') {
                // Try to map back, but simplified:
                const days = dayOfWeek.split(',').map(d => {
                    if (d === 'SUN' || d === '1') return 1;
                    if (d === 'MON' || d === '2') return 2;
                    if (d === 'TUE' || d === '3') return 3;
                    if (d === 'WED' || d === '4') return 4;
                    if (d === 'THU' || d === '5') return 5;
                    if (d === 'FRI' || d === '6') return 6;
                    if (d === 'SAT' || d === '7') return 7;
                    return 0;
                }).filter(d => d !== 0);
                setWeeklyDays(days);
                return;
            }
        }

        // Check Monthly
        // "0 M H D 1/M ?"
        if (dayOfWeek === '?' && day !== '*' && month.includes('/')) {
            setFrequency('monthly');
            setMonthlyTime(`${pad(parseInt(hour))}:${pad(parseInt(min))}`);

            const [startM, intervalM] = month.split('/');
            // Handle "*" or default
            const start = startM === '*' ? 1 : parseInt(startM);
            setMonthlyStartMonth(start);
            setMonthlyInterval(parseInt(intervalM));

            // Check Range "1-10" vs Single "1"
            if (day.includes('-')) {
                const [startD, endD] = day.split('-');
                setMonthlyType('range');
                setMonthlyDay(parseInt(startD));
                setMonthlyEndDay(parseInt(endD));
            } else {
                setMonthlyType('day');
                setMonthlyDay(parseInt(day));
            }
            return;
        }

        // Check Yearly
        // "0 M H D M ?"
        if (dayOfWeek === '?' && day !== '*' && month !== '*' && !month.includes('/')) {
            setFrequency('yearly');
            setYearlyTime(`${pad(parseInt(hour))}:${pad(parseInt(min))}`);
            setYearlyDay(parseInt(day));
            setYearlyMonth(parseInt(month));
            return;
        }

        // Default Custom
        setFrequency('custom');
        setCustomCron(cron);
    };

    // Generate Cron on state change
    useEffect(() => {
        let cron = '';
        if (frequency === 'daily') {
            const [h, m] = dailyTime.split(':');
            if (dailyType === 'once') {
                cron = `0 ${parseInt(m)} ${parseInt(h)} * * ?`;
            } else {
                // Interval: "0 0/10 * * * ?" (Every 10 mins)
                // Simplified: Start at 00:00, end 23:59
                if (dailyIntervalUnit === 'minute') {
                    cron = `0 0/${dailyInterval} * * * ?`;
                } else {
                    cron = `0 0 0/${dailyInterval} * * ?`;
                }
            }
        } else if (frequency === 'weekly') {
            const [h, m] = weeklyTime.split(':');
            const dayStr = weeklyDays.length > 0 ? weeklyDays.join(',') : '?'; // Quartz week days: 1=SUN
            cron = `0 ${parseInt(m)} ${parseInt(h)} ? * ${dayStr}`;
        } else if (frequency === 'monthly') {
            const [h, m] = monthlyTime.split(':');

            let dayStr = '';
            if (monthlyType === 'range') {
                dayStr = `${monthlyDay}-${monthlyEndDay}`;
            } else {
                dayStr = `${monthlyDay}`;
            }

            // Day D of every M months. Start month is tricky in standard cron "1/3".
            // "0 M H D Start/Interval ?"
            cron = `0 ${parseInt(m)} ${parseInt(h)} ${dayStr} ${monthlyStartMonth}/${monthlyInterval} ?`;
        } else if (frequency === 'yearly') {
            const [h, m] = yearlyTime.split(':');
            cron = `0 ${parseInt(m)} ${parseInt(h)} ${yearlyDay} ${yearlyMonth} ?`;
        } else {
            cron = customCron;
        }
        onChange(cron);
    }, [frequency, dailyType, dailyTime, dailyInterval, dailyIntervalUnit, weeklyTime, weeklyDays, monthlyTime, monthlyDay, monthlyEndDay, monthlyInterval, monthlyStartMonth, monthlyType, yearlyTime, yearlyMonth, yearlyDay, customCron]);

    const handleDayCheck = (day: number) => {
        if (weeklyDays.includes(day)) {
            setWeeklyDays(weeklyDays.filter(d => d !== day));
        } else {
            setWeeklyDays([...weeklyDays, day].sort());
        }
    };

    return (
        <div style={{ position: 'fixed', top: '50%', left: '50%', transform: 'translate(-50%, -50%)', backgroundColor: '#ffffff', color: '#333333', padding: '20px', boxShadow: '0 4px 15px rgba(0,0,0,0.5)', zIndex: 1000, minWidth: '500px', borderRadius: '8px' }}>
            <h3 style={{ marginTop: 0, borderBottom: '1px solid #eee', paddingBottom: '10px' }}>排程設定 (Cron Builder)</h3>

            <div style={{ display: 'flex', gap: '10px', marginBottom: '20px', borderBottom: '1px solid #ccc', paddingBottom: '10px' }}>
                <button style={{ backgroundColor: frequency === 'daily' ? '#e3f2fd' : 'transparent', color: '#333', border: '1px solid #ccc', borderRadius: '4px', padding: '6px 12px', cursor: 'pointer' }} onClick={() => setFrequency('daily')}>每日</button>
                <button style={{ backgroundColor: frequency === 'weekly' ? '#e3f2fd' : 'transparent', color: '#333', border: '1px solid #ccc', borderRadius: '4px', padding: '6px 12px', cursor: 'pointer' }} onClick={() => setFrequency('weekly')}>每週</button>
                <button style={{ backgroundColor: frequency === 'monthly' ? '#e3f2fd' : 'transparent', color: '#333', border: '1px solid #ccc', borderRadius: '4px', padding: '6px 12px', cursor: 'pointer' }} onClick={() => setFrequency('monthly')}>每月 (季)</button>
                <button style={{ backgroundColor: frequency === 'yearly' ? '#e3f2fd' : 'transparent', color: '#333', border: '1px solid #ccc', borderRadius: '4px', padding: '6px 12px', cursor: 'pointer' }} onClick={() => setFrequency('yearly')}>每年</button>
                <button style={{ backgroundColor: frequency === 'custom' ? '#e3f2fd' : 'transparent', color: '#333', border: '1px solid #ccc', borderRadius: '4px', padding: '6px 12px', cursor: 'pointer' }} onClick={() => setFrequency('custom')}>自訂</button>
            </div>

            <div style={{ minHeight: '200px' }}>
                {frequency === 'daily' && (
                    <div>
                        <div style={{ marginBottom: '10px' }}>
                            <label>
                                <input type="radio" checked={dailyType === 'once'} onChange={() => setDailyType('once')} />
                                每天於特定時間執行一次:
                            </label>
                            <input type="time" value={dailyTime} onChange={e => setDailyTime(e.target.value)} disabled={dailyType !== 'once'} style={{ marginLeft: '10px' }} />
                        </div>
                        <div>
                            <label>
                                <input type="radio" checked={dailyType === 'interval'} onChange={() => setDailyType('interval')} />
                                每天重複執行，每隔:
                            </label>
                            <input type="number" value={dailyInterval} onChange={e => setDailyInterval(parseInt(e.target.value))} min="1" disabled={dailyType !== 'interval'} style={{ width: '60px', margin: '0 5px' }} />
                            <select value={dailyIntervalUnit} onChange={e => setDailyIntervalUnit(e.target.value as any)} disabled={dailyType !== 'interval'}>
                                <option value="hour">小時</option>
                                <option value="minute">分鐘</option>
                            </select>
                        </div>
                    </div>
                )}

                {frequency === 'weekly' && (
                    <div>
                        <div style={{ marginBottom: '10px' }}>
                            執行時間: <input type="time" value={weeklyTime} onChange={e => setWeeklyTime(e.target.value)} />
                        </div>
                        <div style={{ display: 'flex', gap: '5px' }}>
                            {[{ val: 2, label: '一' }, { val: 3, label: '二' }, { val: 4, label: '三' }, { val: 5, label: '四' }, { val: 6, label: '五' }, { val: 7, label: '六' }, { val: 1, label: '日' }].map(d => (
                                <label key={d.val} style={{ border: '1px solid #ccc', padding: '5px', borderRadius: '4px', backgroundColor: weeklyDays.includes(d.val) ? '#e6f7ff' : 'white', cursor: 'pointer' }}>
                                    <input type="checkbox" checked={weeklyDays.includes(d.val)} onChange={() => handleDayCheck(d.val)} style={{ display: 'none' }} />
                                    {d.label}
                                </label>
                            ))}
                        </div>
                    </div>
                )}

                {frequency === 'monthly' && (
                    <div>
                        <div style={{ marginBottom: '10px' }}>
                            執行時間: <input type="time" value={monthlyTime} onChange={e => setMonthlyTime(e.target.value)} />
                        </div>
                        <div style={{ marginBottom: '10px', display: 'flex', alignItems: 'center', flexWrap: 'wrap' }}>
                            <span style={{ marginRight: '5px' }}>從</span>
                            <select value={monthlyStartMonth} onChange={e => setMonthlyStartMonth(parseInt(e.target.value))} style={{ margin: '0 5px', padding: '2px' }}>
                                {Array.from({ length: 12 }, (_, i) => i + 1).map(m => <option key={m} value={m}>{m}月</option>)}
                            </select>
                            <span style={{ marginRight: '5px' }}>起算，每</span>
                            <input type="number" value={monthlyInterval} onChange={e => setMonthlyInterval(parseInt(e.target.value))} min="1" style={{ width: '50px', marginRight: '5px' }} />
                            個月
                        </div>
                        <div style={{ marginBottom: '5px' }}>
                            <label>
                                <input type="radio" checked={monthlyType === 'day'} onChange={() => setMonthlyType('day')} />
                                指定日期: 第 <input type="number" value={monthlyDay} onChange={e => setMonthlyDay(parseInt(e.target.value))} min="1" max="31" disabled={monthlyType !== 'day'} style={{ width: '50px' }} /> 天
                            </label>
                        </div>
                        <div>
                            <label>
                                <input type="radio" checked={monthlyType === 'range'} onChange={() => setMonthlyType('range')} />
                                日期範圍: 從 <input type="number" value={monthlyDay} onChange={e => setMonthlyDay(parseInt(e.target.value))} min="1" max="31" disabled={monthlyType !== 'range'} style={{ width: '50px' }} />
                                到 <input type="number" value={monthlyEndDay} onChange={e => setMonthlyEndDay(parseInt(e.target.value))} min="1" max="31" disabled={monthlyType !== 'range'} style={{ width: '50px' }} /> 日
                            </label>
                        </div>
                        <div style={{ marginTop: '10px', fontSize: '0.9em', color: '#666' }}>
                            (提示: 若設定每 3 個月，則為季報模式)
                        </div>
                    </div>
                )}

                {frequency === 'yearly' && (
                    <div>
                        <div style={{ marginBottom: '10px' }}>
                            執行時間: <input type="time" value={yearlyTime} onChange={e => setYearlyTime(e.target.value)} />
                        </div>
                        <div>
                            每年
                            <select value={yearlyMonth} onChange={e => setYearlyMonth(parseInt(e.target.value))} style={{ margin: '0 5px' }}>
                                {Array.from({ length: 12 }, (_, i) => i + 1).map(m => <option key={m} value={m}>{m}月</option>)}
                            </select>
                            <input type="number" value={yearlyDay} onChange={e => setYearlyDay(parseInt(e.target.value))} min="1" max="31" style={{ width: '60px' }} /> 日
                        </div>
                    </div>
                )}

                {frequency === 'custom' && (
                    <div>
                        <label>Cron Expression: </label>
                        <input type="text" value={customCron} onChange={e => { setCustomCron(e.target.value); onChange(e.target.value); }} style={{ width: '100%', marginTop: '5px' }} />
                    </div>
                )}
            </div>

            <div style={{ marginTop: '20px', paddingTop: '10px', borderTop: '1px solid #eee' }}>
                <strong>預覽結果: </strong> <span style={{ fontFamily: 'monospace', backgroundColor: '#f5f5f5', padding: '2px 5px' }}>{value}</span>
            </div>

            <div style={{ marginTop: '20px', display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
                <button onClick={onClose} style={{ padding: '8px 16px', backgroundColor: '#2196F3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>確定</button>
            </div>
        </div>
    );
};

export default CronBuilder;
