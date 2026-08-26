import { useCallback, useState } from 'react';
import Alert from '@mui/material/Alert';
import Container from '@mui/material/Container';
import Grid from '@mui/material/Grid2';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { vehicleService, type CatalogItem } from './services/vehicleService';
import { useResource } from './hooks/useResource';
import { ErrorNotice } from './components/ErrorNotice';
import { MakeAutocomplete } from './components/MakeAutocomplete';
import { ModelResults } from './components/ModelResults';
import { VehicleTypeSelect } from './components/VehicleTypeSelect';
import { YearSelect } from './components/YearSelect';

export default function App() {
  const [make, setMake] = useState<CatalogItem | null>(null);
  const [vehicleType, setVehicleType] = useState('');
  const [year, setYear] = useState(new Date().getFullYear());

  const makes = useResource<CatalogItem[]>((signal) => vehicleService.getMakes(signal), []);

  const makeId = make?.id ?? null;

  const vehicleTypes = useResource<CatalogItem[]>(
    (signal) => vehicleService.getVehicleTypes(makeId!, signal),
    [makeId],
    makeId !== null,
  );

  const models = useResource<CatalogItem[]>(
    (signal) => vehicleService.getModels(makeId!, year, vehicleType || null, signal),
    [makeId, year, vehicleType],
    makeId !== null,
  );

  // Vehicle types belong to a make, so changing the make invalidates the chosen type.
  const chooseMake = useCallback((next: CatalogItem | null) => {
    setMake(next);
    setVehicleType('');
  }, []);

  return (
    <Container maxWidth="md" sx={{ py: 5 }}>
      <Stack spacing={4}>
        <Stack spacing={0.5}>
          <Typography variant="h1" component="h1">
            Vehicle Explorer
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Browse the NHTSA vPIC catalogue by make, model year and vehicle type.
          </Typography>
        </Stack>

        {makes.error ? (
          <ErrorNotice error={makes.error} onRetry={makes.reload} />
        ) : (
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, sm: 6 }}>
              <MakeAutocomplete
                makes={makes.data ?? []}
                value={make}
                loading={makes.loading}
                onChange={chooseMake}
              />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <YearSelect value={year} onChange={setYear} />
            </Grid>
            <Grid size={{ xs: 12, sm: 3 }}>
              <VehicleTypeSelect
                vehicleTypes={vehicleTypes.data ?? []}
                value={vehicleType}
                loading={vehicleTypes.loading}
                disabled={makeId === null}
                onChange={setVehicleType}
              />
            </Grid>
          </Grid>
        )}

        {vehicleTypes.error && !makes.error ? (
          <Alert severity="warning">
            Vehicle types could not be loaded, so the type filter is unavailable. Models
            are still listed.
          </Alert>
        ) : null}

        <ModelResults
          models={models.data}
          loading={models.loading}
          error={models.error}
          ready={makeId !== null}
          onRetry={models.reload}
        />
      </Stack>
    </Container>
  );
}
