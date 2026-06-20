import { Card, Typography } from 'antd';
import { useAuth } from '../context/AuthContext';

const { Title, Paragraph } = Typography;

export function DashboardPage() {
  const { user } = useAuth();

  return (
    <Card>
      <Title level={3}>Dashboard</Title>
      <Paragraph>
        Sesión iniciada como <strong>{user?.email}</strong> ({user?.role}).
      </Paragraph>
      
    </Card>
  );
}
