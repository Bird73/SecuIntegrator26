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
                <div style={{ backgroundColor: '#ffffff', borderRadius: '8px', overflow: 'hidden', boxShadow: '0 2px 4px rgba(0,0,0,0.1)' }}>
                    <table style={{ width: '100%', borderCollapse: 'collapse', color: '#333333' }}>
                        <thead>
                            <tr style={{ backgroundColor: '#e0e0e0', color: '#000000' }}>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Group</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Name</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Status</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Cron</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Next Fire</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Last Fire</th>
                                <th style={{ padding: '12px', borderBottom: '2px solid #ccc', textAlign: 'left' }}>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {jobs.map((job, index) => (
                                <tr key={`${job.group}-${job.name}-${index}`} style={{ borderBottom: '1px solid #eee', backgroundColor: index % 2 === 0 ? '#ffffff' : '#f9f9f9' }}>
                                    <td style={{ padding: '12px' }}>{job.group}</td>
                                    <td style={{ padding: '12px', fontWeight: 'bold' }}>{job.name}</td>
                                    <td style={{ padding: '12px' }}>
                                        <span style={{
                                            padding: '4px 8px',
                                            borderRadius: '4px',
                                            backgroundColor: job.triggerState === 'Normal' ? '#e8f5e9' : '#ffebee',
                                            color: job.triggerState === 'Normal' ? '#2e7d32' : '#c62828',
                                            fontWeight: 'bold',
                                            border: '1px solid',
                                            borderColor: job.triggerState === 'Normal' ? '#c8e6c9' : '#ffcdd2'
                                        }}>
                                            {job.triggerState}
                                        </span>
                                    </td>
                                    <td style={{ padding: '12px', fontFamily: 'monospace' }}>{job.cronExpression}</td>
                                    <td style={{ padding: '12px' }}>{job.nextFireTime ? new Date(job.nextFireTime).toLocaleString() : '-'}</td>
                                    <td style={{ padding: '12px' }}>{job.previousFireTime ? new Date(job.previousFireTime).toLocaleString() : '-'}</td>
                                    <td style={{ padding: '12px' }}>
                                        <button onClick={() => handleTrigger(job.name, job.group)} style={{ marginRight: '5px', padding: '5px 10px', cursor: 'pointer', backgroundColor: '#2196F3', color: 'white', border: 'none', borderRadius: '4px' }}>Run Now</button>
                                        {job.triggerState === 'Paused' ? (
                                            <button onClick={() => handleResume(job.name, job.group)} style={{ padding: '5px 10px', cursor: 'pointer', backgroundColor: '#4CAF50', color: 'white', border: 'none', borderRadius: '4px' }}>Resume</button>
                                        ) : (
                                            <button onClick={() => handlePause(job.name, job.group)} style={{ padding: '5px 10px', cursor: 'pointer', backgroundColor: '#9E9E9E', color: 'white', border: 'none', borderRadius: '4px' }}>Pause</button>
                                        )}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table >
                </div>
            )}
        </div >
    );
};

export default SchedulerManager;
