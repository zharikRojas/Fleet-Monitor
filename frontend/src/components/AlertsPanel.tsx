import { Badge, List, Tag, Typography } from 'antd';
import type { Alert } from '../types';

const { Text } = Typography;

interface AlertsPanelProps {
  alerts: Alert[];
  loading: boolean;
  connected: boolean;
}

export function AlertsPanel({ alerts, loading, connected }: AlertsPanelProps) {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 12 }}>
        <Text strong>Alertas predictivas</Text>
        <Badge
          status={connected ? 'processing' : 'default'}
          text={connected ? 'Tiempo real' : 'Sin SignalR'}
        />
      </div>

      <List
        loading={loading}
        locale={{ emptyText: 'No hay alertas de combustible' }}
        dataSource={alerts}
        renderItem={(alert) => (
          <List.Item>
            <List.Item.Meta
              title={
                <span>
                  {alert.device.name}{' '}
                  <Tag color={alert.acknowledged ? 'default' : 'orange'}>
                    {alert.acknowledged ? 'Atendida' : 'Pendiente'}
                  </Tag>
                </span>
              }
              description={
                <>
                  Combustible bajo · ~{alert.estimatedMinutesRemaining.toFixed(0)} min restantes
                  <br />
                  <Text type="secondary">{new Date(alert.createdAt).toLocaleString()}</Text>
                </>
              }
            />
          </List.Item>
        )}
      />
    </div>
  );
}
