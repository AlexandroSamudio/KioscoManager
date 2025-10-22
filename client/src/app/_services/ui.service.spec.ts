import { UiService } from './ui.service';

describe('UiService (state)', () => {
  let service: UiService;

  beforeEach(() => {
    service = new UiService();
    jest.spyOn(window, 'open').mockImplementation(() => null);
    jest.spyOn(console, 'warn').mockImplementation(() => {});
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  it('openExternalLink validates URL and opens window', () => {
    service.openExternalLink('https://example.com');
    expect(window.open).toHaveBeenCalledWith('https://example.com', '_blank', 'noopener,noreferrer');
  });

  it('openExternalLink ignores invalid URL', () => {
    service.openExternalLink('not-a-url');
    expect(window.open).not.toHaveBeenCalled();
    expect(console.warn).toHaveBeenCalledWith('Formato de URL inválido:', 'not-a-url');
  });
});
