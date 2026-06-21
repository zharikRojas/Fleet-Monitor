import { useCallback, useEffect, useMemo, useState } from 'react';
import { Alert, Card, Col, Row, Table, Tag, Typography } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { fetchAlerts } from '../api/alerts';
import { ApiError } from '../api/client';
import { fetchDeviceReadings, fetchDevices } from '../api/devices';
import { AlertsPanel } from '../components/AlertsPanel';
import { DeviceCharts } from '../components/DeviceCharts';
import { DeviceMap } from '../components/DeviceMap';
import { useAuth } from '../context/AuthContext';
import { useAlertsHub } from '../hooks/useAlertsHub';
import { config } from '../lib/config';
import {
  getDevicesCacheTime,
  loadDevicesCache,
  loadReadingsCache,
  saveDevicesCache,
  saveReadingsCache,
} from '../lib/offlineCache';
import type { Alert as FleetAlert, Device, SensorReading } from '../types';

const { Title, Text } = Typography;

function mergeAlerts(base: FleetAlert[], live: FleetAlert[]): FleetAlert[] {
  const map = new Map<string, FleetAlert>();
  for (const alert of base) {
    map.set(alert.id, alert);
  }
  for (const alert of live) {
    map.set(alert.id, alert);
  }
  return [...map.values()].sort(
    (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const isAdmin = user?.role === 'Admin';
  const [devices, setDevices] = useState<Device[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [readings, setReadings] = useState<SensorReading[]>([]);
  const [alerts, setAlerts] = useState<FleetAlert[]>([]);
  const [loadingDevices, setLoadingDevices] = useState(true);
  const [loadingReadings, setLoadingReadings] = useState(false);
  const [loadingAlerts, setLoadingAlerts] = useState(false);
  const [devicesError, setDevicesError] = useState<string | null>(null);
  const [readingsFromCache, setReadingsFromCache] = useState(false);
  const [devicesFromCache, setDevicesFromCache] = useState(false);
  const [cacheTime, setCacheTime] = useState<string | null>(getDevicesCacheTime());
  const [online, setOnline] = useState(navigator.onLine);

  const { liveAlerts, connected } = useAlertsHub(user?.token ?? null, isAdmin);
  const mergedAlerts = useMemo(() => mergeAlerts(alerts, liveAlerts), [alerts, liveAlerts]);
  const selectedDevice = devices.find((device) => device.id === selectedId) ?? null;

  const loadDevices = useCallback(async (options?: { silent?: boolean }) => {
    if (!user) {
      return;
    }

    const silent = options?.silent ?? false;
    if (!silent) {
      setLoadingDevices(true);
      setDevicesError(null);
    }

    try {
      const data = await fetchDevices(user.token);
      setDevices(data);
      saveDevicesCache(data);
      setCacheTime(getDevicesCacheTime());
      setDevicesFromCache(false);
      setSelectedId((current) => current ?? data[0]?.id ?? null);
    } catch (err) {
      const cached = loadDevicesCache();
      setDevices(cached);
      setCacheTime(getDevicesCacheTime());
      setDevicesFromCache(true);
      if (cached.length > 0) {
        setSelectedId((current) => current ?? cached[0]?.id ?? null);
      }
      if (!silent && !cached.length) {
        setDevicesError(
          err instanceof ApiError ? err.message : 'No se pudieron cargar los dispositivos.',
        );
      }
    } finally {
      if (!silent) {
        setLoadingDevices(false);
      }
    }
  }, [user]);

  useEffect(() => {
    function handleOnline() {
      setOnline(true);
    }
    function handleOffline() {
      setOnline(false);
    }

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);
    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  useEffect(() => {
    void loadDevices();
    if (!online) {
      return;
    }

    const intervalId = window.setInterval(() => {
      void loadDevices({ silent: true });
    }, config.devicesPollMs);

    return () => window.clearInterval(intervalId);
  }, [loadDevices, online]);

  useEffect(() => {
    if (!user || !selectedId) {
      setReadings([]);
      return;
    }

    async function loadReadings() {
      setLoadingReadings(true);

      try {
        if (online) {
          const data = await fetchDeviceReadings(user!.token, selectedId!);
          setReadings(data);
          saveReadingsCache(selectedId!, data);
          setReadingsFromCache(false);
        } else {
          const cached = loadReadingsCache(selectedId!);
          setReadings(cached);
          setReadingsFromCache(true);
        }
      } catch {
        const cached = loadReadingsCache(selectedId!);
        setReadings(cached);
        setReadingsFromCache(true);
      } finally {
        setLoadingReadings(false);
      }
    }

    void loadReadings();
  }, [user, selectedId, online]);

  useEffect(() => {
    if (!user || !isAdmin || !online) {
      setAlerts([]);
      return;
    }

    async function loadAlerts() {
      setLoadingAlerts(true);
      try {
        const data = await fetchAlerts(user!.token);
        setAlerts(data);
      } catch {
        setAlerts([]);
      } finally {
        setLoadingAlerts(false);
      }
    }

    void loadAlerts();
  }, [user, isAdmin, online]);

  const columns: ColumnsType<Device> = [
    {
      title: 'ID',
      dataIndex: 'id',
      key: 'id',
      render: (id: string) => id,
    },
    {
      title: 'Vehículo',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => (
        <button
          type="button"
          onClick={() => setSelectedId(record.id)}
          style={{
            border: 'none',
            background: 'transparent',
            padding: 0,
            cursor: 'pointer',
            color: selectedId === record.id ? '#1677ff' : 'inherit',
            fontWeight: selectedId === record.id ? 600 : 400,
          }}
        >
          {name}
        </button>
      ),
    },
    {
      title: 'Comb.',
      dataIndex: 'fuelLevel',
      key: 'fuelLevel',
      render: (fuel: number) => `${fuel.toFixed(0)} L`,
    },
    {
      title: 'Vel.',
      dataIndex: 'speed',
      key: 'speed',
      render: (speed: number) => `${speed.toFixed(0)}`,
    },
  ];

  return (
    <div>
      <div style={{ marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>
          Dashboard
        </Title>
        <Text type="secondary">
          {user?.email} · {user?.role}
          {online ? ' · en línea' : ' · sin conexión'}
        </Text>
      </div>

      {(!online || devicesFromCache) && (
        <Alert
          type={online ? 'info' : 'warning'}
          showIcon
          style={{ marginBottom: 16 }}
          title={
            online
              ? 'Mostrando dispositivos desde caché local'
              : 'Modo offline: mapa, vehículos y gráficos desde localStorage'
          }
          description={
            cacheTime ? `Última sincronización: ${new Date(cacheTime).toLocaleString()}` : undefined
          }
        />
      )}

      {devicesError && (
        <Alert type="error" showIcon title={devicesError} style={{ marginBottom: 16 }} />
      )}

      <Row gutter={[16, 16]}>
        <Col xs={24} lg={isAdmin ? 16 : 24}>
          <Card title="Mapa en vivo" loading={loadingDevices}>
            <DeviceMap
              devices={devices}
              selectedId={selectedId}
              onSelect={setSelectedId}
            />
          </Card>
        </Col>

        {isAdmin && (
          <Col xs={24} lg={8}>
            <Card title="Alertas predictivas" extra={<Tag color="red">Solo admin</Tag>}>
              <AlertsPanel
                alerts={mergedAlerts}
                loading={loadingAlerts}
                connected={connected}
              />
            </Card>
          </Col>
        )}

        <Col xs={24} lg={isAdmin ? 10 : 12}>
          <Card title="Flota" loading={loadingDevices}>
            <Table
              rowKey="id"
              size="small"
              columns={columns}
              dataSource={devices}
              pagination={false}
              onRow={(record) => ({
                onClick: () => setSelectedId(record.id),
                style: {
                  cursor: 'pointer',
                  background: selectedId === record.id ? '#e6f4ff' : undefined,
                },
              })}
            />
          </Card>
        </Col>

        <Col xs={24} lg={isAdmin ? 14 : 12}>
          <Card
            title={
              selectedDevice
                ? `Histórico · ${selectedDevice.name}`
                : 'Histórico · selecciona un vehículo'
            }
          >
            <DeviceCharts
              deviceName={selectedDevice?.name ?? '—'}
              readings={readings}
              loading={loadingReadings}
              fromCache={readingsFromCache}
            />
          </Card>
        </Col>
      </Row>
    </div>
  );
}
