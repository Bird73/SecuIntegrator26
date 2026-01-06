import client from './client';

export interface HolidayConfig {
    date: string; // ISO DateTime string
    description: string;
    isHoliday: boolean;
}

export const getHolidays = async (year: number): Promise<HolidayConfig[]> => {
    const response = await client.get<HolidayConfig[]>(`/holiday/${year}`);
    return response.data;
};

export const addHoliday = async (date: string, description: string): Promise<void> => {
    await client.post('/holiday', { date, description, isHoliday: true });
};

export const removeHoliday = async (date: string): Promise<void> => {
    // API expects DateTime in URL, ensure format matches
    await client.delete(`/holiday/${date}`);
};

export const importHolidays = async (jsonFilePath: string): Promise<void> => {
    await client.post('/holiday/import', { jsonFilePath });
};

export const exportHolidays = async (year: number, jsonFilePath: string): Promise<void> => {
    await client.post(`/holiday/export/${year}`, { jsonFilePath });
};

export const downloadHolidaysFile = async (year: number): Promise<void> => {
    const response = await client.get(`/holiday/download/${year}`, { responseType: 'blob' });
    const url = window.URL.createObjectURL(new Blob([response.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `holidays_${year}.json`);
    document.body.appendChild(link);
    link.click();
    link.remove();
};

export const uploadHolidaysFile = async (file: File): Promise<void> => {
    const formData = new FormData();
    formData.append('file', file);
    await client.post('/holiday/upload', formData, {
        headers: {
            'Content-Type': 'multipart/form-data'
        }
    });
};
