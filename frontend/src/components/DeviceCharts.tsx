import { Empty, Spin, Typography } from 'antd';
import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import type { SensorReading } from '../types';

const { Text } = Typography;

interface DeviceChartsProps {
  deviceName: string;
  readings: SensorReading[];
  loading: boolean;
  fromCache?: boolean;
}

export function DeviceCharts({
  deviceName,
  readings,
  loading,
  fromCache = false,
}: DeviceChartsProps) {
  const chartData = readings.map((reading) => ({
    time: new Date(reading.timestamp).toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit',
    }),
    fuel: reading.fuel,
    speed: reading.speed,
  }));

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: 48 }}>
        <Spin />
      </div>
    );
  }

  if (chartData.length === 0) {
    return <Empty description="Sin lecturas históricas para este vehículo" />;
  }

  return (
    <div>
      <Text type="secondary">
        Histórico de {deviceName}
        {fromCache ? ' · datos en caché' : ''}
      </Text>
      <ResponsiveContainer width="100%" height={280}>
        <LineChart data={chartData} margin={{ top: 16, right: 16, left: 0, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="time" minTickGap={24} />
          <YAxis yAxisId="fuel" orientation="left" unit=" L" />
          <YAxis yAxisId="speed" orientation="right" unit=" km/h" />
          <Tooltip />
          <Legend />
          <Line
            yAxisId="fuel"
            type="monotone"
            dataKey="fuel"
            name="Combustible (L)"
            stroke="#1677ff"
            dot={false}
            strokeWidth={2}
          />
          <Line
            yAxisId="speed"
            type="monotone"
            dataKey="speed"
            name="Velocidad (km/h)"
            stroke="#52c41a"
            dot={false}
            strokeWidth={2}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
