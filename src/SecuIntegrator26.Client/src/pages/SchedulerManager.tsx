import React, { useEffect, useState } from 'react';
import { getJobs, triggerJob, pauseJob, resumeJob, type JobStatusDto } from '../api/scheduler';
import SchedulerConfigForm from '../components/SchedulerConfigForm';

const SchedulerManager: React.FC = () => {
    const [jobs, setJobs] = useState<JobStatusDto[]>([]);
    const [loading, setLoading] = useState(false);
    const [showConfig, setShowConfig] = useState(false);

    const fetchJobs = async () => {
        if (showConfig) return; // Don't poll status when editing config
        setLoading(true);
        try {
            const data = await getJobs();
            setJobs(data);
        } catch (error) {
            console.error(error);
            // alert('Failed to fetch jobs'); // Suppress alert for polling
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchJobs();
        const interval = setInterval(fetchJobs, 5000); // Poll every 5s
        return () => clearInterval(interval);
    }, [showConfig]); // Re-run effect when mode changes

    const handleTrigger = async (name: string, group: string) => {
        if (!window.confirm(`確定要立即執行 ${name} 嗎？`)) return;
        try {
            await triggerJob(name, group);
            alert('已觸發執行');
            fetchJobs();
        } catch (error) {
            alert('操作失敗');
        }
    };

    const handlePause = async (name: string, group: string) => {
        try {
            await pauseJob(name, group);
            fetchJobs();
        } catch (error) {
            alert('操作失敗');
        }
    };

    const handleResume = async (name: string, group: string) => {
        try {
            await resumeJob(name, group);
            fetchJobs();
        } catch (error) {
            alert('操作失敗');
        }
    };

    return (
        <div style={{ padding: '20px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <h2>排程任務監控</h2>
                <div>
                    <button onClick={() => setShowConfig(!showConfig)} style={{ marginRight: '10px' }}>
                        {showConfig ? '回到列表' : '排程設定'}
                    </button>
                    {!showConfig && <button onClick={fetchJobs} disabled={loading}>重新整理</button>}
                </div>
            </div>

            {showConfig ? (
                <SchedulerConfigForm onClose={() => setShowConfig(false)} />
            ) : (
                <table style={{ width: '100%', borderCollapse: 'collapse', border: '1px solid #ddd' }}>
                    <thead>
                        <tr style={{ backgroundColor: '#f8f9fa' }}>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Group</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Name</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Status</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Cron</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Next Fire</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Last Fire</th>
                            <th style={{ padding: '10px', border: '1px solid #ddd' }}>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {jobs.map((job, index) => (
                            <tr key={`${job.group}-${job.name}-${index}`}>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>{job.group}</td>
                                <td style={{ padding: '10px', border: '1px solid #ddd', fontWeight: 'bold' }}>{job.name}</td>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>
                                    <span style={{
                                        padding: '4px 8px',
                                        borderRadius: '4px',
                                        backgroundColor: job.triggerState === 'Normal' ? '#e8f5e9' : '#ffebee',
                                        color: job.triggerState === 'Normal' ? '#2e7d32' : '#c62828'
                                    }}>
                                        {job.triggerState}
                                    </span>
                                </td>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>{job.cronExpression}</td>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>{job.nextFireTime ? new Date(job.nextFireTime).toLocaleString() : '-'}</td>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>{job.previousFireTime ? new Date(job.previousFireTime).toLocaleString() : '-'}</td>
                                <td style={{ padding: '10px', border: '1px solid #ddd' }}>
                                    <button onClick={() => handleTrigger(job.name, job.group)} style={{ marginRight: '5px' }}>Run Now</button>
                                    {job.triggerState === 'Paused' ? (
                                        <button onClick={() => handleResume(job.name, job.group)}>Resume</button>
                                    ) : (
                                        <button onClick={() => handlePause(job.name, job.group)}>Pause</button>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table >
            )}
        </div >
    );
};

export default SchedulerManager;
