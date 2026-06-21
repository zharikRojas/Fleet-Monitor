import { useState } from 'react';
import { LockOutlined, MailOutlined, TruckOutlined } from '@ant-design/icons';
import { Alert, Button, Card, Col, Form, Grid, Input, Row, Typography } from 'antd';
import { Navigate, useNavigate } from 'react-router-dom';
import { login as loginRequest } from '../api/auth';
import { ApiError } from '../api/client';
import { useAuth } from '../context/AuthContext';
import './LoginPage.css';

const { Title, Paragraph } = Typography;
const { useBreakpoint } = Grid;

interface LoginFormValues {
  email: string;
  password: string;
}

export function LoginPage() {
  const { user, login } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const screens = useBreakpoint();
  const isMobile = !screens.md;

  if (user) {
    return <Navigate to="/" replace />;
  }

  async function handleSubmit({ email, password }: LoginFormValues) {
    setError(null);
    setLoading(true);

    try {
      const authUser = await loginRequest(email, password);
      login(authUser);
      navigate('/');
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError(
          'No se pudo conectar con la API.',
        );
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      <div className="login-shell">
        <Card className="login-card" variant="borderless">
          <Row gutter={[32, 32]} align="middle">
            <Col xs={24} md={10} lg={9}>
              <div className="login-brand">
                <div className="login-logo">
                  <TruckOutlined />
                </div>
                <Title level={isMobile ? 4 : 3} className="login-title">
                  Fleet Monitor
                </Title>
                <Paragraph className="login-subtitle">
                  Inicia sesión para monitorear tu flota en tiempo real.
                </Paragraph>
              </div>
            </Col>

            <Col xs={24} md={14} lg={15}>
              {error && (
                <Alert
                  title={error}
                  type="error"
                  showIcon
                  className="login-alert"
                />
              )}

              <Form
                layout="vertical"
                requiredMark={false}
                initialValues={{
                  email: '',
                  password: '',
                }}
                onFinish={handleSubmit}
                size={isMobile ? 'middle' : 'large'}
              >
                <Form.Item
                  label="Email"
                  name="email"
                  rules={[
                    { required: true, message: 'Ingresa tu email' },
                    { type: 'email', message: 'Email no válido' },
                  ]}
                >
                  <Input
                    prefix={<MailOutlined />}
                    placeholder=""
                    autoComplete="off"
                  />
                </Form.Item>

                <Form.Item
                  label="Contraseña"
                  name="password"
                  rules={[{ required: true, message: 'Ingresa tu contraseña' }]}
                >
                  <Input.Password
                    prefix={<LockOutlined />}
                    placeholder=""
                    autoComplete="off"
                  />
                </Form.Item>

                <Form.Item className="login-submit">
                  <Button type="primary" htmlType="submit" block loading={loading}>
                    Ingresar
                  </Button>
                </Form.Item>
              </Form>

              
            </Col>
          </Row>
        </Card>
      </div>
    </div>
  );
}
