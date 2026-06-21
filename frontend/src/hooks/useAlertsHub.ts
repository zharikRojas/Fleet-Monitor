import * as signalR from '@microsoft/signalr';
import { notification } from 'antd';
import { useEffect, useState, type Dispatch, type SetStateAction } from 'react';
import { config } from '../lib/config';
import { normalizeAlert } from '../lib/normalizeAlert';
import type { Alert } from '../types';

function handleIncomingAlert(
  payload: Record<string, unknown>,
  setLiveAlerts: Dispatch<SetStateAction<Alert[]>>,
) {
  const alert = normalizeAlert(payload);

  if (!alert.id) {
    return;
  }

  setLiveAlerts((current) => {
    if (current.some((item) => item.id === alert.id)) {
      return current;
    }
    return [alert, ...current];
  });

  notification.warning({
    message: 'Nueva alerta de combustible',
    description: `${alert.device.name} · ~${alert.estimatedMinutesRemaining.toFixed(0)} min restantes`,
    placement: 'topRight',
  });
}

export function useAlertsHub(token: string | null, enabled: boolean) {
  const [liveAlerts, setLiveAlerts] = useState<Alert[]>([]);
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    if (!enabled || !token) {
      setConnected(false);
      return;
    }

    let cancelled = false;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${config.hubUrl}?access_token=${encodeURIComponent(token)}`)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Legacy name kept for backwards compatibility if an old API instance is still running.
    connection.on('NewAlert', (payload: Record<string, unknown>) => {
      handleIncomingAlert(payload, setLiveAlerts);
    });
    connection.on('Nueva alerta', (payload: Record<string, unknown>) => {
      handleIncomingAlert(payload, setLiveAlerts);
    });

    connection
      .start()
      .then(() => {
        if (!cancelled) {
          setConnected(true);
        }
      })
      .catch((error) => {
        if (!cancelled) {
          console.error('SignalR connection failed:', error);
          setConnected(false);
        }
      });

    connection.onreconnected(() => {
      if (!cancelled) {
        setConnected(true);
      }
    });
    connection.onclose(() => {
      if (!cancelled) {
        setConnected(false);
      }
    });

    return () => {
      cancelled = true;
      void connection.stop();
      setConnected(false);
    };
  }, [enabled, token]);

  return { liveAlerts, connected };
}
