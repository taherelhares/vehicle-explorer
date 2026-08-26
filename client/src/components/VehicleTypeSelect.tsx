import MenuItem from '@mui/material/MenuItem';
import Select from '@mui/material/Select';
import type { CatalogItem } from '../services/vehicleService';
import { FilterField, plainControlSx } from './FilterField';

interface Props {
  vehicleTypes: CatalogItem[];
  value: string;
  loading: boolean;
  disabled: boolean;
  /** True when the types request failed, which is not the same as a make having none. */
  failed: boolean;
  onChange: (vehicleType: string) => void;
}

/** A handful of options per make, so a plain select is the right control here. */
export function VehicleTypeSelect({
  vehicleTypes,
  value,
  loading,
  disabled,
  failed,
  onChange,
}: Props) {
  const empty = !loading && !failed && vehicleTypes.length === 0;
  const unusable = disabled || loading || failed || empty;

  // An empty list is an answer; a failed request is not, so the two never share wording.
  const helper = disabled
    ? 'Choose a make first'
    : failed
      ? 'Could not be loaded'
      : loading
        ? 'Loading types'
        : empty
          ? 'None recorded for this make'
          : 'Optional';

  return (
    <FilterField label="Vehicle type" labelId="vehicle-type-label" helper={helper}>
      <Select
        labelId="vehicle-type-label"
        variant="standard"
        fullWidth
        displayEmpty
        disabled={unusable}
        renderValue={(selected) => (unusable ? '—' : selected || 'All types')}
        value={vehicleTypes.some((t) => t.name === value) ? value : ''}
        onChange={(event) => onChange(event.target.value)}
        sx={plainControlSx}
      >
        <MenuItem value="">All types</MenuItem>
        {vehicleTypes.map((type) => (
          <MenuItem key={type.id} value={type.name}>
            {type.name}
          </MenuItem>
        ))}
      </Select>
    </FilterField>
  );
}
