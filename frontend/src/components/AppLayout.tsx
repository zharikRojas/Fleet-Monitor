import { Button, Layout, Typography } from 'antd';
import { Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const { Header, Content } = Layout;
const { Text } = Typography;

export function AppLayout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate('/login');
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          background: '#001529',
        }}
      >
        <Text style={{ color: '#fff', fontSize: 18, fontWeight: 600 }}>Fleet Monitor</Text>
        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <Text style={{ color: 'rgba(255,255,255,0.85)' }}>
            {user?.email} · {user?.role}
          </Text>
          <Button onClick={handleLogout}>Salir</Button>
        </div>
      </Header>
      <Content style={{ padding: 24 }}>
        <Outlet />
      </Content>
    </Layout>
  );
}
