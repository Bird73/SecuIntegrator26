import React, { useEffect, useState } from 'react';
import { getSchedulerConfig, updateSchedulerConfig, type JobScheduleConfig, type JobSetting } from '../api/scheduler';
import CronBuilder from './CronBuilder';

interface Props {
    onClose: () => void;
}

const SchedulerConfigForm: React.FC<Props> = ({ onClose }) => {
    const [config, setConfig] = useState<JobScheduleConfig | null>(null);
    const [loading, setLoading] = useState(false);

    // Track which job and which cron index we are editing
    // editingIndex = -1 means adding new cron
    const [editingState, setEditingState] = useState<{ jobIndex: number, cronIndex: number } | null>(null);

    useEffect(() => {
        loadConfig();
    }, []);

    const loadConfig = async () => {
        setLoading(true);
        try {
            const data = await getSchedulerConfig();
            // Ensure cronExpressions is populated
            data.jobs.forEach(j => {
                if (!j.cronExpressions) j.cronExpressions = [];
                if (j.cronExpressions.length === 0 && j.cronExpression) {
                    j.cronExpressions.push(j.cronExpression);
                }
            });
            setConfig(data);
        } catch (error) {
            console.error(error);
            alert('Failed to load configuration');
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        if (!config) return;
        setLoading(true);
        try {
            // Update legacy field for compatibility
            config.jobs.forEach(j => {
                j.cronExpression = j.cronExpressions[0] || '';
            });
            await updateSchedulerConfig(config);
            alert('設定已儲存並生效');
            onClose();
        } catch (error) {
            console.error(error);
            alert('Failed to save configuration');
        } finally {
            setLoading(false);
        }
    };

    const toggleEnabled = (index: number) => {
        if (!config) return;
        const newJobs = [...config.jobs];
        newJobs[index].isEnabled = !newJobs[index].isEnabled;
        setConfig({ ...config, jobs: newJobs });
    };

    const updateCron = (newCron: string) => {
        if (!config || !editingState) return;
        const { jobIndex, cronIndex } = editingState;
        const newJobs = [...config.jobs];

        // Update existing only (Adding is handled by handleAddSchedule)
        if (cronIndex >= 0 && cronIndex < newJobs[jobIndex].cronExpressions.length) {
            newJobs[jobIndex].cronExpressions[cronIndex] = newCron;
            setConfig({ ...config, jobs: newJobs });
        }
    };

    const handleAddSchedule = (jobIndex: number) => {
        if (!config) return;
        const newJobs = [...config.jobs];
        // Add default cron
        newJobs[jobIndex].cronExpressions.push('0 0 12 1 * ?');
        setConfig({ ...config, jobs: newJobs });

        // Immediately edit the new item
        setEditingState({
            jobIndex: jobIndex,
            cronIndex: newJobs[jobIndex].cronExpressions.length - 1
        });
    };

    const removeCron = (jobIndex: number, cronIndex: number) => {
        if (!config) return;
        if (!window.confirm('確定要刪除此排程規則嗎？')) return;

        const newJobs = [...config.jobs];
        newJobs[jobIndex].cronExpressions.splice(cronIndex, 1);
        setConfig({ ...config, jobs: newJobs });
    };

    if (!config) return <div>Loading...</div>;

    return (
        <div style={{ padding: '20px', border: '1px solid #ccc', borderRadius: '8px', marginTop: '20px', backgroundColor: '#ffffff', color: '#333333' }}>
            <h3>排程參數設定</h3>
            <div style={{ maxHeight: '600px', overflowY: 'auto' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '20px', color: '#333333' }}>
                    <thead>
                        <tr style={{ textAlign: 'left', backgroundColor: '#f0f0f0', color: '#333333' }}>
                            <th style={{ padding: '8px', borderBottom: '2px solid #ddd', width: '20%' }}>Job Name</th>
                            <th style={{ padding: '8px', borderBottom: '2px solid #ddd', width: '10%' }}>State</th>
                            <th style={{ padding: '8px', borderBottom: '2px solid #ddd' }}>Schedules</th>
                        </tr>
                    </thead>
                    <tbody>
                        {config.jobs.map((job, index) => (
                            <tr key={`${job.groupName}-${job.jobName}`} style={{ borderBottom: '1px solid #eee', verticalAlign: 'top' }}>
                                <td style={{ padding: '8px' }}>
                                    <div style={{ fontWeight: 'bold' }}>{job.jobName}</div>
                                    <div style={{ fontSize: '0.9em', color: '#666' }}>{job.description}</div>
                                </td>
                                <td style={{ padding: '8px' }}>
                                    <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
                                        <input
                                            type="checkbox"
                                            checked={job.isEnabled}
                                            onChange={() => toggleEnabled(index)}
                                            style={{ marginRight: '5px' }}
                                        />
                                        {job.isEnabled ? '啟用' : '停用'}
                                    </label>
                                </td>
                                <td style={{ padding: '8px' }}>
                                    <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
                                        {job.cronExpressions.map((cron, cIndex) => (
                                            <div key={cIndex} style={{ display: 'flex', alignItems: 'center', backgroundColor: '#f9f9f9', padding: '4px', borderRadius: '4px', border: '1px solid #eee' }}>
                                                <div style={{ fontFamily: 'monospace', flex: 1, marginRight: '10px' }}>{cron}</div>
                                                <button
                                                    onClick={() => setEditingState({ jobIndex: index, cronIndex: cIndex })}
                                                    style={{ marginRight: '5px', padding: '2px 6px', fontSize: '0.8em', cursor: 'pointer', backgroundColor: '#e0e0e0', border: '1px solid #ccc' }}
                                                >
                                                    編輯
                                                </button>
                                                <button
                                                    onClick={() => removeCron(index, cIndex)}
                                                    style={{ padding: '2px 6px', fontSize: '0.8em', color: 'white', backgroundColor: '#ff4444', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
                                                >
                                                    刪除
                                                </button>
                                            </div>
                                        ))}
                                        <button
                                            onClick={() => handleAddSchedule(index)}
                                            style={{ alignSelf: 'flex-start', marginTop: '5px', padding: '4px 8px', fontSize: '0.9em', backgroundColor: '#e3f2fd', border: '1px solid #90caf9', borderRadius: '4px', cursor: 'pointer', color: '#1976d2' }}
                                        >
                                            + 新增排程
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div style={{ display: 'flex', gap: '10px', paddingTop: '10px', borderTop: '1px solid #eee' }}>
                <button onClick={handleSave} disabled={loading} style={{ padding: '8px 16px', backgroundColor: '#4RAF50', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    儲存設定
                </button>
                <button onClick={onClose} disabled={loading} style={{ padding: '8px 16px', backgroundColor: '#f44336', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                    取消
                </button>
            </div>

            {editingState !== null && config && (
                <>
                    <div style={{ position: 'fixed', top: 0, left: 0, right: 0, bottom: 0, backgroundColor: 'rgba(0,0,0,0.3)', zIndex: 999 }} onClick={() => setEditingState(null)}></div>
                    <CronBuilder
                        value={config.jobs[editingState.jobIndex].cronExpressions[editingState.cronIndex]}
                        onChange={updateCron}
                        onClose={() => setEditingState(null)}
                    />
                </>
            )}
        </div>
    );
};

export default SchedulerConfigForm;
