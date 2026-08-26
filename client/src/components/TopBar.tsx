import Box from '@mui/material/Box';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';

/** The amber tick: the mark that identifies the product, the results, and focus. */
export function Tick({ height = 16 }: { height?: number }) {
  return (
    <Box
      aria-hidden
      sx={{
        width: 3,
        height,
        borderRadius: 0.5,
        backgroundColor: 'secondary.main',
        flexShrink: 0,
      }}
    />
  );
}

export function TopBar() {
  return (
    <Box
      component="header"
      sx={{
        position: 'sticky',
        top: 0,
        zIndex: (t) => t.zIndex.appBar,
        backgroundColor: 'background.default',
        borderBottom: '1px solid',
        borderColor: 'divider',
      }}
    >
      <Container
        maxWidth="md"
        sx={{ height: 56, display: 'flex', alignItems: 'center', gap: 1.25 }}
      >
        <Tick />
        <Typography sx={{ fontWeight: 600, letterSpacing: '-0.01em' }}>
          Vehicle Explorer
        </Typography>
        <Box sx={{ flex: 1 }} />
        <Typography variant="overline" sx={{ color: 'text.secondary' }}>
          NHTSA vPIC
        </Typography>
      </Container>
    </Box>
  );
}
