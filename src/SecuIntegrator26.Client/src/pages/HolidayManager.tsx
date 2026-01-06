import React, { useEffect, useState } from 'react';
import Calendar from '../components/Calendar';
import { getHolidays, addHoliday, removeHoliday, importHolidays, exportHolidays, type HolidayConfig, uploadHolidaysFile, downloadHolidaysFile } from '../api/holiday';

const HolidayManager: React.FC = () => {
    const [year, setYear] = useState<number>(new Date().getFullYear());
    const [holidayMap, setHolidayMap] = useState<Map<string, string>>(new Map());
    const [loading, setLoading] = useState(false);

    const fetchHolidays = async () => {
        setLoading(true);
        try {
            const data = await getHolidays(year);
            const map = new Map<string, string>();
            data.filter(h => h.isHoliday).forEach(h => {
                map.set(h.date.split('T')[0], h.description);
            });
            setHolidayMap(map);
        } catch (error) {
            console.error(error);
            alert('Failed to fetch holidays');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchHolidays();
    }, [year]);

    const handleDateClick = async (dateStr: string) => {
        const description = holidayMap.get(dateStr);
        try {
            if (description) {
                if (!window.confirm(`確定取消 ${dateStr} 「${description}」的休市設定？`)) return;
                await removeHoliday(dateStr);
            } else {
                const desc = prompt(`設定 ${dateStr} 為休市日，請輸入原因：`, '國定假日');
                if (desc === null) return;
                await addHoliday(dateStr, desc);
            }
            fetchHolidays(); // Refresh
        } catch (error) {
            alert('Operation failed');
        }
    };

    const fileInputRef = React.useRef<HTMLInputElement>(null);

    const handleImportClick = () => {
        fileInputRef.current?.click();
    };

    const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        try {
            await uploadHolidaysFile(file);
            alert('匯入成功');
            fetchHolidays();
        } catch (error) {
            console.error(error);
            alert('匯入失敗');
        } finally {
            // Reset input
            if (event.target) event.target.value = '';
        }
    };

    const handleExport = async () => {
        try {
            await downloadHolidaysFile(year);
        } catch (error) {
            alert('匯出失敗');
        }
    };

    return (
        <div style={{ padding: '20px' }}>
            <h2>股市休市日管理</h2>

            <div style={{ marginBottom: '20px', display: 'flex', gap: '10px', alignItems: 'center' }}>
                <button onClick={() => setYear(year - 1)}>← 上一年</button>
                <span style={{ fontSize: '1.2em', fontWeight: 'bold' }}>{year} 年</span>
                <button onClick={() => setYear(year + 1)}>下一年 →</button>
                <div style={{ flex: 1 }}></div>
                <input
                    type="file"
                    ref={fileInputRef}
                    style={{ display: 'none' }}
                    accept=".json"
                    onChange={handleFileChange}
                />
                <button onClick={handleImportClick}>匯入 (JSON)</button>
                <button onClick={handleExport}>匯出 (JSON)</button>
            </div>

            {loading ? <div>Loading...</div> : (
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '20px', justifyContent: 'center' }}>
                    {Array.from({ length: 12 }, (_, i) => i + 1).map(month => (
                        <Calendar
                            key={month}
                            year={year}
                            month={month}
                            holidays={new Set(holidayMap.keys())}
                            onDateClick={handleDateClick}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default HolidayManager;
