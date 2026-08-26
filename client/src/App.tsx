import { useCallback, useState } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import { vehicleService, type CatalogItem } from './services/vehicleService';
import { useResource } from './hooks/useResource';
import { ErrorNotice } from './components/ErrorNotice';
import { MakeAutocomplete } from './components/MakeAutocomplete';
import { ModelResults } from './components/ModelResults';
import { TopBar } from './components/TopBar';
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
    <Box sx={{ minHeight: '100dvh', backgroundColor: 'background.default' }}>
      <TopBar />

      <Container maxWidth="md" component="main" sx={{ py: { xs: 4, sm: 6 } }}>
        <Stack spacing={{ xs: 3, sm: 4 }}>
          <Stack spacing={1} sx={{ maxWidth: 520 }}>
            <Typography variant="h1" component="h1">
              Find a vehicle model
            </Typography>
            <Typography variant="body1" sx={{ color: 'text.secondary' }}>
              Browse the NHTSA vPIC catalogue by make, model year and vehicle type.
            </Typography>
          </Stack>

          {makes.error ? (
            <ErrorNotice error={makes.error} onRetry={makes.reload} />
          ) : (
            // One plate holds the whole query: the three controls read as a cluster of
            // instruments rather than three unrelated form fields.
            <Paper
              variant="outlined"
              sx={{
                display: 'flex',
                flexDirection: { xs: 'column', md: 'row' },
                overflow: 'hidden',
                '& > *': { flex: { md: 1 } },
                '& > *:first-of-type': { flex: { md: 2 } },
                '& > * + *': {
                  borderTop: { xs: '1px solid', md: 'none' },
                  borderLeft: { xs: 'none', md: '1px solid' },
                  borderColor: 'divider',
                },
              }}
            >
              <MakeAutocomplete
                makes={makes.data ?? []}
                value={make}
                loading={makes.loading}
                onChange={chooseMake}
              />
              <YearSelect value={year} onChange={setYear} />
              <VehicleTypeSelect
                vehicleTypes={vehicleTypes.data ?? []}
                value={vehicleType}
                loading={vehicleTypes.loading}
                disabled={makeId === null}
                failed={vehicleTypes.error !== null}
                onChange={setVehicleType}
              />
            </Paper>
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
            query={{ make: make?.name ?? null, year, vehicleType }}
            onRetry={models.reload}
          />
        </Stack>
      </Container>
    </Box>
  );
}
