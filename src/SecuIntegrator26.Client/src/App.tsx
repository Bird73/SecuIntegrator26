import { Routes, Route, Link } from 'react-router-dom';
import HolidayManager from './pages/HolidayManager';
import SchedulerManager from './pages/SchedulerManager';

function App() {
  return (
    <div className="app-container">
      <nav style={{ padding: '10px', borderBottom: '1px solid #ccc', marginBottom: '20px' }}>
        <Link to="/" style={{ marginRight: '15px' }}>首頁</Link>
        <Link to="/holidays" style={{ marginRight: '15px' }}>休市日管理</Link>
        <Link to="/scheduler">排程監控</Link>
      </nav>

      <Routes>
        <Route path="/" element={<div style={{ padding: '20px' }}><h1>歡迎使用證交所爬蟲系統</h1><p>請從上方選單選擇功能。</p></div>} />
        <Route path="/holidays" element={<HolidayManager />} />
        <Route path="/scheduler" element={<SchedulerManager />} />
      </Routes>
    </div>
  );
}

export default App;
