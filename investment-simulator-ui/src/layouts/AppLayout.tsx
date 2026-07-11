import { Link, Outlet } from 'react-router-dom';
import { AppNav } from '@/components/navigation/AppNav';
import { paths } from '@/routes/paths';
import styles from './AppLayout.module.css';

export function AppLayout() {
  return (
    <div className={styles.shell}>
      <header className={styles.header}>
        <div className={styles.headerInner}>
          <Link to={paths.home} className={styles.brand}>
            Simulador de Investimentos
          </Link>
          <AppNav />
        </div>
      </header>
      <main className={styles.main}>
        <Outlet />
      </main>
    </div>
  );
}
