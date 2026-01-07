import client from './client';

export interface JobStatusDto {
    group: string;
    name: string;
    description: string;
    status: string;
    nextFireTime: string | null;
    previousFireTime: string | null;
    triggerState: string;
    cronExpression: string;
}

export interface JobSetting {
    jobName: string;
    groupName: string;
    cronExpression: string;
    cronExpressions: string[];
    isEnabled: boolean;
    description: string;
}

export interface JobScheduleConfig {
    jobs: JobSetting[];
}

export const getJobs = async (): Promise<JobStatusDto[]> => {
    const response = await client.get<JobStatusDto[]>('/scheduler');
    return response.data;
};

export const triggerJob = async (name: string, group: string): Promise<void> => {
    await client.post(`/scheduler/trigger?name=${name}&group=${group}`);
};

export const pauseJob = async (name: string, group: string): Promise<void> => {
    await client.post(`/scheduler/pause?name=${name}&group=${group}`);
};

export const resumeJob = async (name: string, group: string): Promise<void> => {
    await client.post(`/scheduler/resume?name=${name}&group=${group}`);
};

export const getSchedulerConfig = async (): Promise<JobScheduleConfig> => {
    const response = await client.get<JobScheduleConfig>('/scheduler/config');
    return response.data;
};

export const updateSchedulerConfig = async (config: JobScheduleConfig): Promise<void> => {
    await client.post('/scheduler/config', config);
};
