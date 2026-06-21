import { useEffect, useRef, useState } from 'react';
import Map, { Marker, NavigationControl, Popup } from 'react-map-gl/maplibre';
import 'maplibre-gl/dist/maplibre-gl.css';
import { config } from '../lib/config';
import type { Device } from '../types';

const DEFAULT_VIEW = {
  latitude: 4.6533,
  longitude: -74.0836,
  zoom: 11,
};

interface DeviceMapProps {
  devices: Device[];
  selectedId: string | null;
  onSelect: (deviceId: string | null) => void;
}

function DevicePopupContent({ device }: { device: Device }) {
  return (
    <div style={{ minWidth: 180, fontSize: 13, lineHeight: 1.5 }}>
      <strong style={{ display: 'block', marginBottom: 6 }}>{device.name}</strong>
      <div>
        <span style={{ color: '#666' }}>Lat:</span> {device.lastLat.toFixed(5)}
      </div>
      <div>
        <span style={{ color: '#666' }}>Lng:</span> {device.lastLng.toFixed(5)}
      </div>
      <div>
        <span style={{ color: '#666' }}>Combustible:</span> {device.fuelLevel.toFixed(1)} L
      </div>
      <div>
        <span style={{ color: '#666' }}>Velocidad:</span> {device.speed.toFixed(0)} km/h
      </div>
    </div>
  );
}

export function DeviceMap({ devices, selectedId, onSelect }: DeviceMapProps) {
  const hasCenteredRef = useRef(false);
  const [viewState, setViewState] = useState(DEFAULT_VIEW);
  const selectedDevice = devices.find((device) => device.id === selectedId) ?? null;

  // Centrar solo la primera vez que hay dispositivos; no al refrescar cada 15s
  useEffect(() => {
    if (devices.length === 0 || hasCenteredRef.current) {
      return;
    }

    hasCenteredRef.current = true;
    const latitude = devices.reduce((sum, d) => sum + d.lastLat, 0) / devices.length;
    const longitude = devices.reduce((sum, d) => sum + d.lastLng, 0) / devices.length;

    setViewState((current) => ({
      ...current,
      latitude,
      longitude,
    }));
  }, [devices]);

  return (
    <div style={{ height: 420, borderRadius: 8, overflow: 'hidden' }}>
      <Map
        {...viewState}
        onMove={(event) => setViewState(event.viewState)}
        mapStyle={config.mapStyleUrl}
        style={{ width: '100%', height: '100%' }}
        onClick={() => onSelect(null)}
      >
        <NavigationControl position="top-right" />
        {devices.map((device) => (
          <Marker
            key={device.id}
            latitude={device.lastLat}
            longitude={device.lastLng}
            anchor="bottom"
            onClick={(event) => {
              event.originalEvent.stopPropagation();
              onSelect(device.id);
            }}
          >
            <div
              style={{
                width: 14,
                height: 14,
                borderRadius: '50%',
                background: selectedId === device.id ? '#1677ff' : '#f5222d',
                border: '2px solid #fff',
                boxShadow: '0 2px 6px rgba(0,0,0,0.35)',
                cursor: 'pointer',
              }}
            />
          </Marker>
        ))}
        {selectedDevice && (
          <Popup
            latitude={selectedDevice.lastLat}
            longitude={selectedDevice.lastLng}
            anchor="bottom"
            offset={[0, -12]}
            closeOnClick={false}
            onClose={() => onSelect(null)}
          >
            <DevicePopupContent device={selectedDevice} />
          </Popup>
        )}
      </Map>
    </div>
  );
}
