import { useTranslation } from 'react-i18next';

export default function LangSwitcher() {
  const { i18n } = useTranslation();
  const isAr = i18n.language === 'ar';

  return (
    <button
      onClick={() => i18n.changeLanguage(isAr ? 'en' : 'ar')}
      className="text-xs px-2 py-1 rounded border border-gray-300 dark:border-gray-600 text-gray-600 dark:text-dark-muted hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
      title={isAr ? 'English' : 'العربية'}
    >
      {isAr ? 'EN' : 'عربي'}
    </button>
  );
}
