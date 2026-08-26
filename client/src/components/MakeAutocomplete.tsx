import Autocomplete from '@mui/material/Autocomplete';
import CircularProgress from '@mui/material/CircularProgress';
import SvgIcon from '@mui/material/SvgIcon';
import TextField from '@mui/material/TextField';
import type { CatalogItem } from '../services/vehicleService';
import { FilterField, plainControlSx } from './FilterField';

interface Props {
  makes: CatalogItem[];
  value: CatalogItem | null;
  loading: boolean;
  onChange: (make: CatalogItem | null) => void;
}

function SearchGlyph() {
  return (
    <SvgIcon
      viewBox="0 0 24 24"
      sx={{ fontSize: 18, color: 'text.secondary', mr: 1, flexShrink: 0 }}
    >
      <path
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
        d="M10.5 4a6.5 6.5 0 1 0 0 13 6.5 6.5 0 0 0 0-13ZM15.5 15.5 20 20"
      />
    </SvgIcon>
  );
}

/**
 * vPIC lists thousands of makes, so this is a searchable field rather than a plain
 * select - scrolling that list to find "Honda" is not a real interaction.
 */
export function MakeAutocomplete({ makes, value, loading, onChange }: Props) {
  const helper = loading
    ? 'Loading catalogue'
    : makes.length === 1
      ? '1 make'
      : `${makes.length.toLocaleString('en-US')} makes`;

  return (
    <FilterField label="Make" labelId="make-label" htmlFor="make" helper={helper}>
      <Autocomplete
        id="make"
        options={makes}
        value={value}
        loading={loading}
        onChange={(_, make) => onChange(make)}
        getOptionLabel={(make) => make.name}
        isOptionEqualToValue={(a, b) => a.id === b.id}
        sx={plainControlSx}
        // The list is long enough that rendering every option would stutter; MUI only
        // renders what is filtered, and this keeps the filtered set sane too.
        filterOptions={(options, { inputValue }) => {
          const needle = inputValue.trim().toLowerCase();
          if (!needle) return options.slice(0, 100);
          return options.filter((o) => o.name.toLowerCase().includes(needle)).slice(0, 100);
        }}
        renderInput={(params) => (
          <TextField
            {...params}
            variant="standard"
            placeholder="Search makes"
            slotProps={{
              input: {
                ...params.InputProps,
                startAdornment: <SearchGlyph />,
                endAdornment: (
                  <>
                    {loading ? <CircularProgress size={14} color="inherit" /> : null}
                    {params.InputProps.endAdornment}
                  </>
                ),
              },
            }}
          />
        )}
      />
    </FilterField>
  );
}
